/*
================================================================
SCRIPT 6: SEED TRASH DATA FOR TESTING
================================================================
*/

-- Disable triggers (like logging) for this session
SET session_replication_role = 'replica';

-- Use a DO block to declare variables and manage UUIDs
DO $$
DECLARE
    -- User IDs
    client1_id UUID;
    client2_id UUID;
    lawyer1_id UUID;
    lawyer2_id UUID;
    admin1_id UUID;
    
    -- Entity IDs
    process1_id UUID;
    chat1_id UUID;
    chat2_id UUID;
    appt1_id UUID;
BEGIN

    -- === 1. CREATE USERS & ROLES ===

    -- Client 1: Alice Smith
    INSERT INTO "User" (email, password_hash, "Name", phone, status)
    VALUES ('alice.smith@example.com', '...hash...', 'Alice Smith', 911111111, 'ACTIVE')
    RETURNING "ID" INTO client1_id;
    
    INSERT INTO "Client" ("ID", NIF, address)
    VALUES (client1_id, 111111111, '123 Main St, Anytown');

    -- Client 2: Bob Jones (Inactive)
    INSERT INTO "User" (email, password_hash, "Name", phone, status)
    VALUES ('bob.jones@example.com', '...hash...', 'Bob Jones', 922222222, 'INACTIVE')
    RETURNING "ID" INTO client2_id;
    
    INSERT INTO "Client" ("ID", NIF, address)
    VALUES (client2_id, 222222222, '456 Oak Ave, Othertown');

    -- Lawyer 1: Saul Goodman
    INSERT INTO "User" (email, password_hash, "Name", phone, status)
    VALUES ('saul.goodman@example.com', '...hash...', 'Saul Goodman', 933333333, 'ACTIVE')
    RETURNING "ID" INTO lawyer1_id;

    INSERT INTO "Lawyer" ("ID", NIF, professional_register_number)
    VALUES (lawyer1_id, 333333333, 'PN-12345');

    -- Lawyer 2: Kim Wexler
    INSERT INTO "User" (email, password_hash, "Name", phone, status)
    VALUES ('kim.wexler@example.com', '...hash...', 'Kim Wexler', 944444444, 'ACTIVE')
    RETURNING "ID" INTO lawyer2_id;

    INSERT INTO "Lawyer" ("ID", NIF, professional_register_number)
    VALUES (lawyer2_id, 444444444, 'PN-67890');
    
    -- Admin 1: Admin User
    INSERT INTO "User" (email, password_hash, "Name", phone, status)
    VALUES ('admin@consilium.app', '...hash...', 'Admin User', 960000000, 'ACTIVE')
    RETURNING "ID" INTO admin1_id;
    
    INSERT INTO "Admin" ("ID", start_date)
    VALUES (admin1_id, '2023-01-01');

    RAISE NOTICE 'Created 2 Clients, 2 Lawyers, and 1 Admin';

    -- === 2. CREATE RELATED ENTITIES ===

    -- Process (for Alice + Saul)
    INSERT INTO "Process" (client_ID, lawyer_ID, process_number, title, description, status, current_phase)
    VALUES (client1_id, lawyer1_id, 'PROC-2025-001', 'Civil Case: Smith v. Corp', 'Initial petition for damages.', 'OPEN', 'INITIAL_PETITION')
    RETURNING "ID" INTO process1_id;

    -- Process (for Bob + Kim)
    INSERT INTO "Process" (client_ID, lawyer_ID, process_number, title, description, status, current_phase)
    VALUES (client2_id, lawyer2_id, 'PROC-2025-002', 'Real Estate Dispute', 'Dispute over property lines.', 'PENDING', 'SUMMONS_AND_DEFENSE');

    -- Document (for Alice's process)
    -- We use decode() to insert "fake" byte data for the 'file' column
    INSERT INTO "Documents" (process_ID, file, file_name)
    VALUES (process1_id, decode('This is the text content of a fake PDF file.', 'escape'), 'Initial_Petition.pdf');
    INSERT INTO "Documents" (process_ID, file, file_name)
    VALUES (process1_id, decode('More fake byte data.', 'escape'), 'Evidence_A.jpg');

    RAISE NOTICE 'Created 2 Processes and 2 Documents';

    -- Chat (between Alice + Saul)
    INSERT INTO "Chat" (client_ID, lawyer_ID)
    VALUES (client1_id, lawyer1_id)
    RETURNING "ID" INTO chat1_id;

    -- Chat (between Bob + Saul)
    INSERT INTO "Chat" (client_ID, lawyer_ID)
    VALUES (client2_id, lawyer1_id)
    RETURNING "ID" INTO chat2_id;

    -- Messages (for Chat 1)
    INSERT INTO "Message" (chat_ID, sender_ID, content, sent_at)
    VALUES (chat1_id, lawyer1_id, 'Hi Alice, I have received your documents.', '2025-10-29 10:00:00');
    
    INSERT INTO "Message" (chat_ID, sender_ID, content, sent_at)
    VALUES (chat1_id, client1_id, 'Thanks Saul. When are you free?', '2025-10-29 10:05:00');

    INSERT INTO "Message" (chat_ID, sender_ID, content, sent_at)
    VALUES (chat1_id, lawyer1_id, 'How about next Tuesday?', '2025-10-29 10:06:00');

    RAISE NOTICE 'Created 2 Chats and 3 Messages';
    
    -- Appointment (Alice + Saul)
    INSERT INTO "Appointment" (client_ID, lawyer_ID, process_ID, appointment_time)
    VALUES (client1_id, lawyer1_id, process1_id, '2025-11-05 14:00:00')
    RETURNING "ID" INTO appt1_id;

    -- Appointment Note (by Saul)
    INSERT INTO "Appointment_Notes" (appointment_ID, author_ID, note)
    VALUES (appt1_id, lawyer1_id, 'Client confirmed. Will discuss strategy.');
    
    -- Notification (for Bob)
    INSERT INTO "Notification" (recipient_ID, content, is_read)
    VALUES (client2_id, 'Your process has been updated to PENDING.', false);

    RAISE NOTICE 'Created 1 Appointment, 1 Note, and 1 Notification';

END $$;

-- Re-enable triggers for normal operation
SET session_replication_role = 'origin';