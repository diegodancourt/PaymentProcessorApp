-- Create customers table
CREATE TABLE IF NOT EXISTS customers (
    customer_id UUID PRIMARY KEY,
    email VARCHAR(255) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    phone VARCHAR(50),
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- Create index on email for faster lookups
CREATE INDEX IF NOT EXISTS idx_customers_email ON customers(email);

-- Insert seed data (same as the in-memory data)
INSERT INTO customers (customer_id, email, name, phone) VALUES
    ('11111111-1111-1111-1111-111111111111', 'john.doe@example.com', 'John Doe', '+1-555-0101'),
    ('22222222-2222-2222-2222-222222222222', 'jane.smith@example.com', 'Jane Smith', '+1-555-0102'),
    ('33333333-3333-3333-3333-333333333333', 'bob.johnson@example.com', 'Bob Johnson', '+1-555-0103')
ON CONFLICT (customer_id) DO NOTHING;