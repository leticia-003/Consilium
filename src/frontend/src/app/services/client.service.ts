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

  getClients(params?: any): Observable<any> {
    return this.http.get<any>(`${API_BASE_URL}/clients`, { params });
  }

}
