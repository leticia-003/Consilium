export interface Lawyer {
  id: string;
  name: string;
  email?: string;
  nif?: string;
  professionalRegister?: string; // lawyer professional register / OAB
  phone?: string;
  isActive?: boolean;
  createdAt?: string;
}
