import { Component, inject } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { ButtonModule } from 'primeng/button';
import { finalize } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import { AuthStateService } from '../../../../shared/services/auth-state.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, InputTextModule, PasswordModule, ButtonModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthService);
  private authState = inject(AuthStateService);

  form: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  submitting = false;
  error: string | null = null;

  onSubmit() {
    if (this.form.invalid || this.submitting) return;
    this.submitting = true;
    this.error = null;

    const credentials = this.form.getRawValue() as { email: string; password: string };

    this.authService.login(credentials).pipe(
      finalize(() => {
        this.submitting = false;
      })
    ).subscribe({
      next: (response) => {
        const displayName = `${response.firstName} ${response.lastName}`.trim();
        this.authState.setSession(displayName, response.role, response.token);
        this.router.navigateByUrl('/menu');
      },
      error: (err: unknown) => {
        const message = typeof (err as any)?.error === 'string' ? (err as any).error : 'Login failed. Please try again.';
        this.error = message;
      }
    });
  }
}
