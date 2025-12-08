import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup, AbstractControl, ValidationErrors } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { finalize, switchMap } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { AuthStateService } from '../../../../shared/services/auth-state.service';

function matchPassword(group: AbstractControl): ValidationErrors | null {
  const pwd = group.get('password')?.value;
  const confirm = group.get('confirmPassword')?.value;
  return pwd && confirm && pwd !== confirm ? { passwordMismatch: true } : null;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);
  private authState = inject(AuthStateService);

  form: FormGroup = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.pattern(/^(?=.{8,100}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).*$/)]],
    confirmPassword: ['', [Validators.required]]
  }, { validators: matchPassword });

  submitting = false;
  error: string | null = null;

  onSubmit() {
    if (this.form.invalid || this.submitting) return;
    this.submitting = true;
    this.error = null;

    const { firstName, lastName, email, password } = this.form.getRawValue() as {
      firstName: string;
      lastName: string;
      email: string;
      password: string;
    };

    const payload = {
      firstName,
      lastName,
      email,
      password,
      role: 'Student',
      userName: email
    };

    this.authService.register(payload).pipe(
      switchMap(() => this.authService.login({ email, password })),
      finalize(() => {
        this.submitting = false;
      })
    ).subscribe({
      next: (response) => {
        const displayName = `${response.firstName} ${response.lastName}`.trim();
        this.authState.setSession(displayName, response.token);
        this.router.navigateByUrl('/menu');
      },
      error: (err: unknown) => {
        const message = typeof (err as any)?.error === 'string' ? (err as any).error : 'Registration failed. Please try again.';
        this.error = message;
      }
    });
  }
}
