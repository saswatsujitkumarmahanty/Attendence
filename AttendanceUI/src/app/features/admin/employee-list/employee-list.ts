import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { EmployeeService } from '../../../core/services/employee.service';
import { Employee } from '../../../core/models/employee.model';

@Component({
  standalone: true,
  imports: [CommonModule,RouterLink],
  selector: 'app-employee-list',
  styleUrl: './employee-list.css',
  templateUrl: './employee-list.html',
})
export class EmployeeList implements OnInit {
  readonly employees = signal<Employee[]>([]);
  readonly loading = signal(true);
  readonly errorMessage = signal<string | null>(null);
 
  constructor(private readonly employeeService: EmployeeService) {}
 
  ngOnInit(): void {
    this.load();
  }
 
  load(): void {
    this.loading.set(true);
    this.employeeService.getAll().subscribe({
      next: (employees) => {
        this.employees.set(employees);
        this.loading.set(false);
      },
      error: () => {
        this.errorMessage.set('Could not load employees.');
        this.loading.set(false);
      }
    });
  }
 
  remove(employee: Employee): void {
    if (!confirm(`Delete employee "${employee.employeeName}"?`)) {
      return;
    }
 
    this.employeeService.delete(employee.employeeId).subscribe({
      next: () => this.load(),
      error: () => this.errorMessage.set('Could not delete that employee.')
    });
  }
}
