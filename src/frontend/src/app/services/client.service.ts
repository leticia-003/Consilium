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
  // Quando a API estiver pronta, muda para false e assegura que o HttpClientModule está importado.
  private useMock = false;

  constructor(private http: HttpClient) {}

  getClients(): Observable<Client[]> {
    if (this.useMock) {
      return of(MOCK_CLIENTS).pipe(delay(300));
    }

    return this.http.get<Client[]>(`${API_BASE_URL}/clients`).pipe(
      catchError((err) => {
        console.error('Error fetching clients:', err);
        throw err;
      })
    );
  }
}
