export interface Client {
  // Corresponds to CORE.CLIENT.CLIENT_ID (UUID) / CORE.USER.USER_ID
  id: string;

  // From CORE.USER
  name: string; // USER_NAME
  taxId?: string; // USER_NIF
  email?: string; // USER_EMAIL
  isActive?: boolean; // USER_IS_ACTIVE

  // From CORE.CLIENT
  address?: string; // CLIENT_ADDRESS

  // From CORE.PHONE (main phone)
  phone?: string;

  // Optional metadata
  createdAt?: string; // ISO date
}
