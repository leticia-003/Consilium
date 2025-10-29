/*
================================================================
SCRIPT 1: DROP ALL (Makes the script idempotent)
================================================================
*/
-- Drop triggers and functions first
DROP TRIGGER IF EXISTS trg_check_chat_participants ON "Chat";
DROP FUNCTION IF EXISTS check_chat_participants();
DROP TRIGGER IF EXISTS trg_enforce_process_status ON "Process";
DROP FUNCTION IF EXISTS enforce_process_status();
DROP TRIGGER IF EXISTS trg_validate_message_sender ON "Message";
DROP FUNCTION IF EXISTS validate_message_sender();
DROP TRIGGER IF EXISTS trg_validate_appointment_time ON "Appointment";
DROP FUNCTION IF EXISTS validate_appointment_time();

-- Drop tables (CASCADE drops dependent objects)
DROP TABLE IF EXISTS 
    "User", "Client", "Lawyer", "Admin", 
    "Process", "Documents", "Notification", 
    "Chat", "Message", "Appointment", "Appointment_Notes",
    "Client_Log", "Lawyer_Log", "Admin_Log", "Process_Log"

CASCADE;

-- Drop types
DROP TYPE IF EXISTS user_status CASCADE;
DROP TYPE IF EXISTS process_phase CASCADE;
DROP TYPE IF EXISTS process_status CASCADE;
DROP TYPE IF EXISTS log_operation_user CASCADE;
DROP TYPE IF EXISTS log_operation_admin CASCADE;
DROP TYPE IF EXISTS log_operation_process CASCADE;


/*
================================================================
SCRIPT 2: CREATE TYPES
================================================================
*/
CREATE TYPE user_status AS ENUM ('ACTIVE', 'INACTIVE');
CREATE TYPE process_phase AS ENUM (
    'INITIAL_PETITION', 'SUMMONS_AND_DEFENSE', 'JUDICIAL_ORDER', 
    'PRELIMINARY_HEARING', 'TRIAL_PREPARATION', 'TRIAL', 
    'VEREDICT', 'APPEAL', 'ENFORCEMENT_OF_JUDGMENT'
);
CREATE TYPE process_status AS ENUM ('OPEN', 'CLOSED', 'PENDING');
CREATE TYPE log_operation_user AS ENUM ('CREATE', 'UPDATE', 'DELETE', 'ACTIVATE', 'INACTIVATE');
CREATE TYPE log_operation_admin AS ENUM ('CREATE', 'UPDATE', 'DELETE');
CREATE TYPE log_operation_process AS ENUM ('CREATE', 'UPDATE', 'CLOSE', 'REOPEN', 'DELETE');


/*
================================================================
SCRIPT 3: CREATE TABLES (With Data Type & Constraint Fixes)
================================================================
*/

-- 1. USER
CREATE TABLE "User" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email TEXT UNIQUE NOT NULL
        CHECK (email ~* '^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$'), -- Email validation
    password_hash TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    phone INT UNIQUE
        CHECK (phone >= 100000000 AND phone <= 999999999), -- 9-digit phone number
    status user_status DEFAULT 'ACTIVE' NOT NULL
);

-- 2. CLIENT
CREATE TABLE "Client" (
    "ID" UUID PRIMARY KEY REFERENCES "User"("ID") ON DELETE CASCADE,
    NIF INT NOT NULL UNIQUE
        CHECK (NIF >= 100000000 AND NIF <= 999999999), -- 9-digit NIF
    address TEXT
);

-- 3. LAWYER
CREATE TABLE "Lawyer" (
    "ID" UUID PRIMARY KEY REFERENCES "User"("ID") ON DELETE CASCADE,
    NIF INT NOT NULL UNIQUE
        CHECK (NIF >= 100000000 AND NIF <= 999999999), -- 9-digit NIF
    professional_register_number TEXT UNIQUE NOT NULL
);

-- 4. ADMIN
CREATE TABLE "Admin" (
    "ID" UUID PRIMARY KEY REFERENCES "User"("ID") ON DELETE CASCADE,
    start_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- 5. PROCESS
CREATE TABLE "Process" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_ID UUID NOT NULL REFERENCES "Client"("ID") ON DELETE RESTRICT,
    lawyer_ID UUID NOT NULL REFERENCES "Lawyer"("ID") ON DELETE RESTRICT,
    process_number TEXT NOT NULL UNIQUE,
    title TEXT NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    closed_at TIMESTAMP,
    current_phase process_phase DEFAULT 'INITIAL_PETITION' NOT NULL,
    status process_status DEFAULT 'OPEN' NOT NULL,
    CHECK (closed_at IS NULL OR closed_at >= created_at) -- Ensure logical dates
);

-- 6. DOCUMENTS
CREATE TABLE "Documents" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    process_ID UUID NOT NULL REFERENCES "Process"("ID") ON DELETE CASCADE,
    file BYTEA NOT NULL,
    file_name TEXT NOT NULL,
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    UNIQUE (process_ID, file_name)
);

-- 7. NOTIFICATION
CREATE TABLE "Notification" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    process_ID UUID REFERENCES "Process"("ID") ON DELETE SET NULL,
    recipient_ID UUID NOT NULL REFERENCES "User"("ID") ON DELETE CASCADE,
    content TEXT NOT NULL,
    is_read BOOLEAN DEFAULT FALSE NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 8. CHAT
CREATE TABLE "Chat" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_ID UUID NOT NULL REFERENCES "Client"("ID") ON DELETE RESTRICT,
    lawyer_ID UUID NOT NULL REFERENCES "Lawyer"("ID") ON DELETE RESTRICT,
    UNIQUE (client_ID, lawyer_ID)
);

-- 9. MESSAGE
CREATE TABLE "Message" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    chat_ID UUID NOT NULL REFERENCES "Chat"("ID") ON DELETE CASCADE,
    sender_ID UUID NOT NULL REFERENCES "User"("ID") ON DELETE RESTRICT,
    content TEXT NOT NULL,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 10. APPOINTMENT
CREATE TABLE "Appointment" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_ID UUID NOT NULL REFERENCES "Client"("ID") ON DELETE RESTRICT,
    lawyer_ID UUID NOT NULL REFERENCES "Lawyer"("ID") ON DELETE RESTRICT,
    process_ID UUID REFERENCES "Process"("ID") ON DELETE SET NULL,
    appointment_time TIMESTAMP NOT NULL,
    UNIQUE (lawyer_ID, appointment_time),
    UNIQUE (client_ID, appointment_time)
);

-- 11. APPOINTMENT_NOTES
CREATE TABLE "Appointment_Notes" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    appointment_ID UUID NOT NULL REFERENCES "Appointment"("ID") ON DELETE CASCADE,
    author_ID UUID NOT NULL REFERENCES "User"("ID") ON DELETE RESTRICT,
    note TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 12. CLIENT_LOG
CREATE TABLE "Client_Log" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_ID UUID NOT NULL REFERENCES "Client"("ID") ON DELETE CASCADE,
    operation log_operation_user NOT NULL,
    performed_by UUID NOT NULL REFERENCES "User"("ID") ON DELETE RESTRICT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    old_value JSONB, -- Can be NULL on CREATE
    new_value JSONB  -- Can be NULL on DELETE
);

-- 13. LAWYER_LOG (and so on...)
CREATE TABLE "Lawyer_Log" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    lawyer_ID UUID NOT NULL REFERENCES "Lawyer"("ID") ON DELETE CASCADE,
    operation log_operation_user NOT NULL,
    performed_by UUID NOT NULL REFERENCES "User"("ID") ON DELETE RESTRICT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    old_value JSONB,
    new_value JSONB
);

-- 14. ADMIN_LOG
CREATE TABLE "Admin_Log" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    admin_ID UUID NOT NULL REFERENCES "Admin"("ID") ON DELETE CASCADE,
    operation log_operation_admin NOT NULL,
    performed_by UUID NOT NULL REFERENCES "User"("ID") ON DELETE RESTRICT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    old_value JSONB,
    new_value JSONB
);

-- 15. PROCESS_LOG
CREATE TABLE "Process_Log" (
    "ID" UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    process_ID UUID NOT NULL REFERENCES "Process"("ID") ON DELETE CASCADE,
    operation log_operation_process NOT NULL,
    performed_by UUID NOT NULL REFERENCES "User"("ID") ON DELETE RESTRICT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP NOT NULL,
    old_value JSONB,
    new_value JSONB
);


/*
================================================================
SCRIPT 4: CREATE PERFORMANCE INDEXES (CRITICAL)
================================================================
*/
-- Postgres does NOT automatically index foreign keys. This is vital.

-- Process
CREATE INDEX idx_process_client_id ON "Process"(client_ID);
CREATE INDEX idx_process_lawyer_id ON "Process"(lawyer_ID);

-- Documents
CREATE INDEX idx_documents_process_id ON "Documents"(process_ID);

-- Notification
CREATE INDEX idx_notification_process_id ON "Notification"(process_ID);
CREATE INDEX idx_notification_recipient_id ON "Notification"(recipient_ID);

-- Chat
CREATE INDEX idx_chat_client_id ON "Chat"(client_ID);
CREATE INDEX idx_chat_lawyer_id ON "Chat"(lawyer_ID);

-- Message
CREATE INDEX idx_message_chat_id ON "Message"(chat_ID);
CREATE INDEX idx_message_sender_id ON "Message"(sender_ID);

-- Appointment
CREATE INDEX idx_appointment_client_id ON "Appointment"(client_ID);
CREATE INDEX idx_appointment_lawyer_id ON "Appointment"(lawyer_ID);
CREATE INDEX idx_appointment_process_id ON "Appointment"(process_ID);

-- Appointment_Notes
CREATE INDEX idx_appointmentnotes_app_id ON "Appointment_Notes"(appointment_ID);
CREATE INDEX idx_appointmentnotes_author_id ON "Appointment_Notes"(author_ID);

-- Log Tables
CREATE INDEX idx_clientlog_client_id ON "Client_Log"(client_ID);
CREATE INDEX idx_clientlog_performed_by ON "Client_Log"(performed_by);
CREATE INDEX idx_lawyerlog_lawyer_id ON "Lawyer_Log"(lawyer_ID);
CREATE INDEX idx_lawyerlog_performed_by ON "Lawyer_Log"(performed_by);
CREATE INDEX idx_adminlog_admin_id ON "Admin_Log"(admin_ID);
CREATE INDEX idx_adminlog_performed_by ON "Admin_Log"(performed_by);
CREATE INDEX idx_processlog_process_id ON "Process_Log"(process_ID);
CREATE INDEX idx_processlog_performed_by ON "Process_Log"(performed_by);


/*
================================================================
SCRIPT 5: CREATE TRIGGERS & FUNCTIONS
================================================================
*/

-- TRIGGER 1: Ensure Chat participants are distinct
CREATE OR REPLACE FUNCTION check_chat_participants() RETURNS TRIGGER AS $$
BEGIN
    IF NEW.client_ID = NEW.lawyer_ID THEN
        RAISE EXCEPTION 'Chat participants must be distinct users.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_check_chat_participants
BEFORE INSERT ON "Chat"
FOR EACH ROW EXECUTE FUNCTION check_chat_participants();

---

-- TRIGGER 2: Keep Process status and closed_at date in sync (More Robust)
CREATE OR REPLACE FUNCTION enforce_process_status() RETURNS TRIGGER AS $$
BEGIN
    -- Case 1: Status is changed to 'CLOSED'
    IF TG_OP = 'UPDATE' AND NEW.status = 'CLOSED' AND OLD.status != 'CLOSED' THEN
        NEW.closed_at = CURRENT_TIMESTAMP;
    -- Case 2: Status is changed *from* 'CLOSED'
    ELSIF TG_OP = 'UPDATE' AND NEW.status != 'CLOSED' AND OLD.status = 'CLOSED' THEN
        NEW.closed_at = NULL;
    -- Case 3: closed_at is manually set
    ELSIF TG_OP = 'UPDATE' AND NEW.closed_at IS NOT NULL AND OLD.closed_at IS NULL THEN
        NEW.status = 'CLOSED';
    -- Case 4: closed_at is manually cleared
    ELSIF TG_OP = 'UPDATE' AND NEW.closed_at IS NULL AND OLD.closed_at IS NOT NULL THEN
        NEW.status = 'OPEN';
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_enforce_process_status
BEFORE UPDATE ON "Process"
FOR EACH ROW EXECUTE FUNCTION enforce_process_status();

---

-- !!NEW!! TRIGGER 3: Validate Message sender
CREATE OR REPLACE FUNCTION validate_message_sender() RETURNS TRIGGER AS $$
DECLARE
    chat_client_id UUID;
    chat_lawyer_id UUID;
BEGIN
    -- Get the participants of the chat
    SELECT client_ID, lawyer_ID INTO chat_client_id, chat_lawyer_id
    FROM "Chat"
    WHERE "ID" = NEW.chat_ID;

    -- Check if the sender is one of the participants
    IF NEW.sender_ID != chat_client_id AND NEW.sender_ID != chat_lawyer_id THEN
        RAISE EXCEPTION 'Sender (ID: %) is not a participant in this chat (ID: %).', NEW.sender_ID, NEW.chat_ID;
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validate_message_sender
BEFORE INSERT ON "Message"
FOR EACH ROW EXECUTE FUNCTION validate_message_sender();

---

-- !!NEW!! TRIGGER 4: Validate Appointment time (must be in the future)
CREATE OR REPLACE FUNCTION validate_appointment_time() RETURNS TRIGGER AS $$
BEGIN
    IF NEW.appointment_time <= CURRENT_TIMESTAMP THEN
        RAISE EXCEPTION 'Appointment time must be in the future.';
    END IF;
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

CREATE TRIGGER trg_validate_appointment_time
BEFORE INSERT OR UPDATE ON "Appointment"
FOR EACH ROW EXECUTE FUNCTION validate_appointment_time();