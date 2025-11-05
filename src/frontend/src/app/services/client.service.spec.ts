import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { ClientService } from './client.service';
import { API_BASE_URL } from '../config';

describe('ClientService', () => {
  let service: ClientService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        ClientService,
        provideHttpClient(),
        provideHttpClientTesting(),
        provideZonelessChangeDetection()
      ]
    });
    service = TestBed.inject(ClientService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch clients from API', (done) => {
    const mockClients = {
      data: [
        { id: '1', name: 'John Doe', email: 'john@test.com' },
        { id: '2', name: 'Jane Smith', email: 'jane@test.com' }
      ],
      meta: { totalCount: 2 }
    };

    service.getClients().subscribe((response) => {
      expect(response.data.length).toBe(2);
      expect(response.meta.totalCount).toBe(2);
      expect(response.data[0].name).toBe('John Doe');
      done();
    });

    const req = httpMock.expectOne(`${API_BASE_URL}/clients`);
    expect(req.request.method).toBe('GET');
    req.flush(mockClients);
  });

  it('should fetch clients with query parameters', () => {
    const params = { page: 1, limit: 10, search: 'John' };

    service.getClients(params).subscribe();

    const req = httpMock.expectOne((request) => {
      return request.url === `${API_BASE_URL}/clients` &&
             request.params.get('page') === '1' &&
             request.params.get('limit') === '10' &&
             request.params.get('search') === 'John';
    });
    expect(req.request.method).toBe('GET');
    req.flush({ data: [], meta: { totalCount: 0 } });
  });

  it('should fetch a single client by id', (done) => {
    const mockClient = {
      id: '123',
      name: 'John Doe',
      email: 'john@test.com',
      nif: '123456789',
      address: '123 Main St'
    };

    service.getClient('123').subscribe((response) => {
      expect(response.id).toBe('123');
      expect(response.name).toBe('John Doe');
      done();
    });

    const req = httpMock.expectOne(`${API_BASE_URL}/clients/123`);
    expect(req.request.method).toBe('GET');
    req.flush(mockClient);
  });

  it('should delete a client', (done) => {
    service.deleteClient('123').subscribe((response) => {
      expect(response).toEqual({});
      done();
    });

    const req = httpMock.expectOne(`${API_BASE_URL}/clients/123`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('should handle errors when fetching clients', (done) => {
    service.getClients().subscribe({
      next: () => fail('should have failed with 500 error'),
      error: (error) => {
        expect(error.status).toBe(500);
        done();
      }
    });

    const req = httpMock.expectOne(`${API_BASE_URL}/clients`);
    req.flush('Server error', { status: 500, statusText: 'Internal Server Error' });
  });
});
