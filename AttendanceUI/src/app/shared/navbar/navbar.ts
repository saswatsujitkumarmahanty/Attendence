import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  selector: 'app-navbar',
  styleUrl: './navbar.css',
  templateUrl: './navbar.html',
})
export class Navbar {
  constructor(
    protected readonly auth: AuthService,
    private readonly router: Router
  ) {}
 
  logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
