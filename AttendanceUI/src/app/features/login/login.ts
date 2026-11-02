import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {ReactiveFormsModule, FormBuilder, Validators} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  selector: 'app-login',
  styleUrl: './login.css',
  templateUrl: './login.html',
})
export class Login {

  readonly submitting = signal(false);
readonly errorMessage = signal<string | null>(null);
readonly showPassword = signal(false);

togglePasswordVisibility(): void {
  this.showPassword.update((value) => !value);
} 

  form: ReturnType<FormBuilder['group']>;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.form = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required]
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const { username, password } = this.form.getRawValue();

    this.authService.login({
      username: username ?? '',
      password: password ?? ''
    }).subscribe({
      next: () => {
        this.submitting.set(false);
        this.router.navigate(['/punch']);
      },

      error: (err: any) => {
        this.submitting.set(false);
        this.errorMessage.set(
          err?.error?.message ?? 'Invalid username or password.'
        );
      }
    });
  }
}