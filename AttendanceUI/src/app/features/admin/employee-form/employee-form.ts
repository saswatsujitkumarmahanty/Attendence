import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  selector: 'app-employee-form',
  styleUrl: './employee-form.css',
  templateUrl: './employee-form.html',
})
export class EmployeeForm implements OnInit {

  readonly isEditMode = signal(false);
  readonly submitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  private employeeId: number | null = null;

  readonly form;

  constructor(
    private readonly fb: FormBuilder,
    private readonly employeeService: EmployeeService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {
    this.form = this.fb.nonNullable.group({
      employeeCode: ['', Validators.required],
      employeeName: ['', Validators.required],
      departmentId: [1, [Validators.required, Validators.min(1)]]
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');

    if (idParam) {
      this.employeeId = Number(idParam);
      this.isEditMode.set(true);

      this.employeeService.getById(this.employeeId).subscribe({
        next: (employee) => {
          this.form.patchValue({
            employeeCode: employee.employeeCode ?? '',
            employeeName: employee.employeeName ?? '',
            departmentId: employee.departmentId ?? 1
          });
        },

        error: () => {
          this.errorMessage.set('Could not load that employee.');
        }
      });
    }
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.errorMessage.set(null);

    const value = this.form.getRawValue();

    if (this.isEditMode() && this.employeeId !== null) {

      this.employeeService.update(this.employeeId, value).subscribe({
        next: () => {
          this.submitting.set(false);
          this.router.navigate(['/admin/employees']);
        },

        error: (err: any) => {
          this.submitting.set(false);
          this.errorMessage.set(
            err?.error?.message ?? 'Could not save that employee.'
          );
        }
      });

    } else {

      this.employeeService.create(value).subscribe({
        next: () => {
          this.submitting.set(false);
          this.router.navigate(['/admin/employees']);
        },

        error: (err: any) => {
          this.submitting.set(false);
          this.errorMessage.set(
            err?.error?.message ?? 'Could not save that employee.'
          );
        }
      });
    }
  }
}