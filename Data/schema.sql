-- Petrol Station Management System schema (idempotent: safe to re-run)
-- psql -U postgres -h localhost -d psms -f Data/schema.sql

CREATE TABLE IF NOT EXISTS roles (
    id   SERIAL PRIMARY KEY,
    name VARCHAR(30) NOT NULL UNIQUE
);

CREATE TABLE IF NOT EXISTS users (
    id            SERIAL PRIMARY KEY,
    username      VARCHAR(50)  NOT NULL UNIQUE,
    password_hash VARCHAR(100) NOT NULL,
    full_name     VARCHAR(100) NOT NULL,
    role_id       INT NOT NULL REFERENCES roles(id),
    is_active     BOOLEAN NOT NULL DEFAULT TRUE,
    created_at    TIMESTAMP NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS fuel_products (
    id        SERIAL PRIMARY KEY,
    code      VARCHAR(10)  NOT NULL UNIQUE,
    name      VARCHAR(50)  NOT NULL UNIQUE,
    unit      VARCHAR(10)  NOT NULL DEFAULT 'L',
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS tanks (
    id            SERIAL PRIMARY KEY,
    code          VARCHAR(20) NOT NULL UNIQUE,
    product_id    INT NOT NULL REFERENCES fuel_products(id),
    capacity      NUMERIC(12,2) NOT NULL CHECK (capacity > 0),
    current_stock NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (current_stock >= 0),
    low_threshold NUMERIC(12,2) NOT NULL DEFAULT 0 CHECK (low_threshold >= 0),
    is_active     BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT ck_tank_stock_cap CHECK (current_stock <= capacity)
);

CREATE TABLE IF NOT EXISTS pumps (
    id      SERIAL PRIMARY KEY,
    code    VARCHAR(20) NOT NULL UNIQUE,
    tank_id INT NOT NULL REFERENCES tanks(id),
    status  VARCHAR(15) NOT NULL DEFAULT 'Active'
            CHECK (status IN ('Active','Inactive','Maintenance'))
);

-- Price history: the latest row with effective_from <= now() is the current price
CREATE TABLE IF NOT EXISTS fuel_prices (
    id             SERIAL PRIMARY KEY,
    product_id     INT NOT NULL REFERENCES fuel_products(id),
    price          NUMERIC(12,2) NOT NULL CHECK (price > 0),
    effective_from TIMESTAMP NOT NULL DEFAULT now(),
    set_by         INT NOT NULL REFERENCES users(id)
);
CREATE INDEX IF NOT EXISTS ix_fuel_prices_prod ON fuel_prices(product_id, effective_from DESC);

CREATE TABLE IF NOT EXISTS suppliers (
    id        SERIAL PRIMARY KEY,
    name      VARCHAR(100) NOT NULL UNIQUE,
    phone     VARCHAR(30),
    email     VARCHAR(100),
    address   VARCHAR(200),
    is_active BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS deliveries (
    id          SERIAL PRIMARY KEY,
    supplier_id INT NOT NULL REFERENCES suppliers(id),
    product_id  INT NOT NULL REFERENCES fuel_products(id),
    tank_id     INT NOT NULL REFERENCES tanks(id),
    quantity    NUMERIC(12,2) NOT NULL CHECK (quantity > 0),
    unit_cost   NUMERIC(12,2) NOT NULL CHECK (unit_cost >= 0),
    invoice_ref VARCHAR(50),
    received_by INT NOT NULL REFERENCES users(id),
    received_at TIMESTAMP NOT NULL DEFAULT now(),
    is_void     BOOLEAN NOT NULL DEFAULT FALSE,
    void_reason VARCHAR(300),
    voided_by   INT REFERENCES users(id),
    voided_at   TIMESTAMP
);
CREATE INDEX IF NOT EXISTS ix_deliveries_date ON deliveries(received_at);

CREATE TABLE IF NOT EXISTS shifts (
    id          SERIAL PRIMARY KEY,
    attendant_id INT NOT NULL REFERENCES users(id),
    opened_at   TIMESTAMP NOT NULL DEFAULT now(),
    closed_at   TIMESTAMP,
    status      VARCHAR(10) NOT NULL DEFAULT 'Open' CHECK (status IN ('Open','Closed'))
);
-- Only one open shift at a time
CREATE UNIQUE INDEX IF NOT EXISTS ux_one_open_shift ON shifts(status) WHERE status = 'Open';

CREATE TABLE IF NOT EXISTS pump_readings (
    id              SERIAL PRIMARY KEY,
    shift_id        INT NOT NULL REFERENCES shifts(id),
    pump_id         INT NOT NULL REFERENCES pumps(id),
    opening_reading NUMERIC(14,2) NOT NULL CHECK (opening_reading >= 0),
    closing_reading NUMERIC(14,2),
    UNIQUE (shift_id, pump_id),
    CONSTRAINT ck_reading CHECK (closing_reading IS NULL OR closing_reading >= opening_reading)
);

CREATE TABLE IF NOT EXISTS customers (
    id           SERIAL PRIMARY KEY,
    name         VARCHAR(100) NOT NULL UNIQUE,
    phone        VARCHAR(30),
    credit_limit NUMERIC(14,2) NOT NULL DEFAULT 0 CHECK (credit_limit >= 0),
    balance      NUMERIC(14,2) NOT NULL DEFAULT 0 CHECK (balance >= 0),
    is_active    BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS sales (
    id          SERIAL PRIMARY KEY,
    shift_id    INT NOT NULL REFERENCES shifts(id),
    pump_id     INT NOT NULL REFERENCES pumps(id),
    product_id  INT NOT NULL REFERENCES fuel_products(id),
    litres      NUMERIC(12,2) NOT NULL CHECK (litres > 0),
    unit_price  NUMERIC(12,2) NOT NULL CHECK (unit_price > 0),   -- price used at the time
    total       NUMERIC(14,2) NOT NULL CHECK (total >= 0),
    created_at  TIMESTAMP NOT NULL DEFAULT now(),
    is_void     BOOLEAN NOT NULL DEFAULT FALSE,
    void_reason VARCHAR(300),
    voided_by   INT REFERENCES users(id),
    voided_at   TIMESTAMP
);
CREATE INDEX IF NOT EXISTS ix_sales_date ON sales(created_at);
CREATE INDEX IF NOT EXISTS ix_sales_shift ON sales(shift_id);

-- kind 'Sale' = payment for fuel sold; 'CreditReceipt' = customer paying off credit
CREATE TABLE IF NOT EXISTS payments (
    id          SERIAL PRIMARY KEY,
    shift_id    INT REFERENCES shifts(id),
    sale_id     INT REFERENCES sales(id),
    customer_id INT REFERENCES customers(id),
    kind        VARCHAR(15) NOT NULL DEFAULT 'Sale' CHECK (kind IN ('Sale','CreditReceipt')),
    method      VARCHAR(15) NOT NULL CHECK (method IN ('Cash','Mobile Money','Card','Credit')),
    amount      NUMERIC(14,2) NOT NULL CHECK (amount > 0),
    reference   VARCHAR(80),
    paid_at     TIMESTAMP NOT NULL DEFAULT now(),
    recorded_by INT NOT NULL REFERENCES users(id),
    is_void     BOOLEAN NOT NULL DEFAULT FALSE,
    void_reason VARCHAR(300),
    voided_by   INT REFERENCES users(id),
    voided_at   TIMESTAMP,
    CONSTRAINT ck_credit_customer CHECK (method <> 'Credit' OR customer_id IS NOT NULL)
);
CREATE INDEX IF NOT EXISTS ix_payments_shift ON payments(shift_id);

CREATE TABLE IF NOT EXISTS expenses (
    id           SERIAL PRIMARY KEY,
    category     VARCHAR(40) NOT NULL,
    amount       NUMERIC(14,2) NOT NULL CHECK (amount > 0),
    payee        VARCHAR(100) NOT NULL,
    description  VARCHAR(300),
    expense_date DATE NOT NULL DEFAULT CURRENT_DATE,
    recorded_by  INT NOT NULL REFERENCES users(id),
    created_at   TIMESTAMP NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_expenses_date ON expenses(expense_date);

CREATE TABLE IF NOT EXISTS audit_logs (
    id         SERIAL PRIMARY KEY,
    user_id    INT REFERENCES users(id),
    username   VARCHAR(50),
    action     VARCHAR(40) NOT NULL,
    details    VARCHAR(500),
    created_at TIMESTAMP NOT NULL DEFAULT now()
);
CREATE INDEX IF NOT EXISTS ix_audit_date ON audit_logs(created_at);

-- ---------------- Seed data ----------------
INSERT INTO roles(name) VALUES ('Admin'),('Manager'),('Cashier'),('Attendant')
ON CONFLICT (name) DO NOTHING;

-- BCrypt hashes (admin/Admin@123, manager/Manager@123, cashier/Cashier@123, attendant/Attendant@123)
INSERT INTO users(username, password_hash, full_name, role_id) VALUES
 ('admin',    '$2a$11$L8iPBTlVTdAXMo/vjm3dT.hjOGaYXnLFaQGvZDz3Ew.k3FM2en/sS', 'System Administrator', (SELECT id FROM roles WHERE name='Admin')),
 ('manager',  '$2a$11$LI7y3B6Fm3xbwj.MqsqXteVfSn9a1g/svOxD6CWhMDK/AWwao89y6', 'Station Manager',     (SELECT id FROM roles WHERE name='Manager')),
 ('cashier',  '$2a$11$oL143ArruuhnWjv9mkBsc.RkWwssk0gbblfJ/y9oLA/u2C7JgXfgu', 'Main Cashier',        (SELECT id FROM roles WHERE name='Cashier')),
 ('attendant','$2a$11$s.UCobFSdfhUzu2lbWtjROjP4CiO1hKGp1anYYEEZMJEhRADlyIvK', 'Pump Attendant',      (SELECT id FROM roles WHERE name='Attendant'))
ON CONFLICT (username) DO NOTHING;

INSERT INTO fuel_products(code, name, unit) VALUES ('PET','Petrol','L'),('DSL','Diesel','L')
ON CONFLICT (code) DO NOTHING;

INSERT INTO tanks(code, product_id, capacity, current_stock, low_threshold) VALUES
 ('TANK-PET-1', (SELECT id FROM fuel_products WHERE code='PET'), 20000, 12000, 3000),
 ('TANK-DSL-1', (SELECT id FROM fuel_products WHERE code='DSL'), 20000, 10000, 3000)
ON CONFLICT (code) DO NOTHING;

INSERT INTO pumps(code, tank_id) VALUES
 ('P1-PET', (SELECT id FROM tanks WHERE code='TANK-PET-1')),
 ('P2-PET', (SELECT id FROM tanks WHERE code='TANK-PET-1')),
 ('P3-DSL', (SELECT id FROM tanks WHERE code='TANK-DSL-1')),
 ('P4-DSL', (SELECT id FROM tanks WHERE code='TANK-DSL-1'))
ON CONFLICT (code) DO NOTHING;

INSERT INTO fuel_prices(product_id, price, effective_from, set_by)
SELECT p.id, v.price, now() - interval '1 day', (SELECT id FROM users WHERE username='manager')
FROM (VALUES ('PET', 1800.00), ('DSL', 1750.00)) v(code, price)
JOIN fuel_products p ON p.code = v.code
WHERE NOT EXISTS (SELECT 1 FROM fuel_prices);

INSERT INTO suppliers(name, phone, email, address) VALUES
 ('Kigali Fuel Distributors', '+250788000001', 'sales@kfd.example', 'Kigali'),
 ('Great Lakes Petroleum',    '+250788000002', 'info@glp.example',  'Rubavu')
ON CONFLICT (name) DO NOTHING;

INSERT INTO customers(name, phone, credit_limit) VALUES
 ('Rwanda Transport Ltd', '+250788100001', 500000),
 ('City Taxi Cooperative', '+250788100002', 200000)
ON CONFLICT (name) DO NOTHING;
