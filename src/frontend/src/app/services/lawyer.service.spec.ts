import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideZonelessChangeDetection } from '@angular/core';
import { LawyerService } from './lawyer.service';
import { environment } from '../../environments/environment';

describe('LawyerService', () => {
  let service: LawyerService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        LawyerService,
        provideHttpClient(),
        provideHttpClientTesting(),
        provideZonelessChangeDetection(),
      ],
    });
    service = TestBed.inject(LawyerService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should fetch lawyers from API', (done) => {
    const mockLawyers = {
      data: [{ id: '1', name: 'Lawyer 1' }],
      meta: { totalCount: 1 },
    };

    service.getLawyers().subscribe((res) => {
      expect(res.data.length).toBe(1);
      expect(res.data[0].name).toBe('Lawyer 1');
      done();
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/lawyers`);
    expect(req.request.method).toBe('GET');
    req.flush(mockLawyers);
  });

  it('should fetch a single lawyer by id', (done) => {
    const mockLawyer = { id: '123', name: 'Lawyer A' };

    service.getLawyer('123').subscribe((resp) => {
      expect(resp.id).toBe('123');
      done();
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/lawyers/123`);
    expect(req.request.method).toBe('GET');
    req.flush(mockLawyer);
  });

  it('should delete a lawyer', (done) => {
    service.deleteLawyer('123').subscribe((resp) => {
      expect(resp).toEqual({});
      done();
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/lawyers/123`);
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('should create a lawyer', (done) => {
    const payload = { name: 'New Lawyer', email: 'new@test.com', password: 'Password123!', professionalRegister: 'PR-1' };
    service.createLawyer(payload).subscribe((resp) => {
      expect(resp).toEqual({});
      done();
    });

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/lawyers`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body.name).toBe('New Lawyer');
    req.flush({});
  });
});
