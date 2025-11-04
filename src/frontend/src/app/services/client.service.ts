import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { delay, catchError } from 'rxjs/operators';
import { Client } from '../models/client';
import { MOCK_CLIENTS } from '../pages/clients/mock-clients';
import { API_BASE_URL } from '../config';

@Injectable({ providedIn: 'root' })
export class ClientService {
  // Toggle aqui para usar mocks ou a API real.
  // Quando a API estiver pronta, mudar para false e assegurar que o HttpClientModule está importado.
  private useMock = false;

  constructor(private http: HttpClient) {}

  getClients(params?: any): Observable<any> {
    if (this.useMock) {
      return of({ data: MOCK_CLIENTS, meta: { totalCount: MOCK_CLIENTS.length } }).pipe(delay(250));
    }
    return this.http.get<any>(`${API_BASE_URL}/clients`, { params });
  }

  getClient(id: string): Observable<any> {
    if (this.useMock) {
      const found = MOCK_CLIENTS.find(c => c.id === id);
      return of(found).pipe(delay(200));
    }
    return this.http.get<any>(`${API_BASE_URL}/clients/${id}`);
  }

}
