import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  Validators
} from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import {
  UserRole,
  CreateUserRequest
} from '../../core/models/user.model';

@Component({
  standalone: true,
  selector: 'app-register',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class Register {

  readonly submitting = signal(false);
readonly successMessage = signal<string | null>(null);
readonly errorMessage = signal<string | null>(null);
readonly showPassword = signal(false);

togglePasswordVisibility(): void {
  this.showPassword.update((value) => !value);
}

  readonly roles: UserRole[] = [
    'Admin',
    'Manager',
    'Employee'
  ];

  readonly form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.form = this.fb.nonNullable.group({
      username: ['', Validators.required],

      password: ['', [
        Validators.required,
        Validators.minLength(6)
      ]],

      role: ['Employee' as UserRole, Validators.required],

      employeeId: [null as number | null]
    });
  }

  submit(): void {

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const value = this.form.getRawValue();

    const request: CreateUserRequest = {
      username: value.username.trim(),
      password: value.password,
      role: value.role,
      employeeId:
        value.employeeId !== null &&
        value.employeeId !== undefined &&
        value.employeeId > 0
          ? Number(value.employeeId)
          : null
    };

    console.log('Register request:', request);

    this.authService.register(request).subscribe({

      next: (response) => {

        console.log('Registration successful:', response);

        this.submitting.set(false);

        this.router.navigate(['/login'], {
          queryParams: {
            registered: 'true'
          }
        });
      },

      error: (err) => {

        console.error('Registration failed:', err);

        this.submitting.set(false);

        this.errorMessage.set(
          err?.error?.message ??
          'Could not create the user.'
        );
      }
    });
  }
}
