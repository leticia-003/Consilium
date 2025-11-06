import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.html',
  styleUrls: ['./login.css'],
  imports: [ReactiveFormsModule, CommonModule],
  standalone: true
})

export class LoginComponent { 
  loginForm: FormGroup;
  errorMessage: string | null = null;
  loading = false;
  private readonly apiUrl = 'http://localhost:8080/api/auth/login';

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private http: HttpClient
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      return;
    }

    this.loading = true;
    this.errorMessage = null;

    const { email, password } = this.loginForm.value;
    
    this.http.post<{ token: string }>(this.apiUrl, { email, password })
      .pipe(
        catchError(error => {
          this.errorMessage = 'Login failed. Please check your credentials.';
          this.loading = false;
          console.error('Login API Error:', error);
          return of(null); 
        })
      )
      .subscribe(response => {
        this.loading = false;
        
        if (response && response.token) {
          localStorage.setItem('auth_token', response.token); 
          
          this.router.navigate(['/home']);
        }
        
      });
  }
}