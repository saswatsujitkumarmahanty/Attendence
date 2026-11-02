import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environmentApiUrl } from '../api-config';
import {
  LoginRequest,
  LoginResponse,CreateUserRequest,CreateUserResponse,
  UserRole
} from '../models/user.model';

interface StoredSession {
  token: string;
  userId: number;
  username: string;
  role: UserRole;
  employeeId: number | null;
}

const STORAGE_KEY = 'attendance.session';

@Injectable({ providedIn: 'root' })
export class AuthService {

  private readonly sessionSignal = signal<StoredSession | null>(
    this.readStoredSession()
  );

  readonly session = this.sessionSignal.asReadonly();

  readonly isLoggedIn = computed(
    () => this.sessionSignal() !== null
  );

  readonly role = computed(
    () => this.sessionSignal()?.role ?? null
  );

  readonly username = computed(
    () => this.sessionSignal()?.username ?? null
  );

  readonly employeeId = computed(
    () => this.sessionSignal()?.employeeId ?? null
  );

  readonly isAdmin = computed(
    () => this.role() === 'Admin'
  );

  readonly isManagerOrAdmin = computed(
    () =>
      this.role() === 'Admin' ||
      this.role() === 'Manager'
  );

  constructor(
    private readonly http: HttpClient
  ) {}

  register(request: CreateUserRequest): Observable<CreateUserResponse> {
  return this.http.post<CreateUserResponse>(
    `${environmentApiUrl}/Auth/register`,
    request
  );
}

  login(request: LoginRequest): Observable<LoginResponse> {

    return this.http
      .post<LoginResponse>(
        `${environmentApiUrl}/Auth/login`,
        request
      )
      .pipe(
        tap((response) => {

          const session: StoredSession = {
            token: response.token,
            userId: response.userId,
            username: response.username,
            role: response.role,
            employeeId: this.readEmployeeIdFromToken(
              response.token
            )
          };

          // Only access localStorage in the browser
          if (typeof localStorage !== 'undefined') {
            localStorage.setItem(
              STORAGE_KEY,
              JSON.stringify(session)
            );
          }

          this.sessionSignal.set(session);
        })
      );
  }

  logout(): void {

    if (typeof localStorage !== 'undefined') {
      localStorage.removeItem(STORAGE_KEY);
    }

    this.sessionSignal.set(null);
  }

  getToken(): string | null {
    return this.sessionSignal()?.token ?? null;
  }

  private readStoredSession(): StoredSession | null {

    // localStorage does not exist during SSR/server-side execution
    if (typeof localStorage === 'undefined') {
      return null;
    }

    const raw = localStorage.getItem(STORAGE_KEY);

    if (!raw) {
      return null;
    }

    try {
      return JSON.parse(raw) as StoredSession;
    } catch {
      return null;
    }
  }

  private readEmployeeIdFromToken(
    token: string
  ): number | null {

    try {

      const parts = token.split('.');

      if (parts.length !== 3) {
        return null;
      }

      const payload = JSON.parse(
        atob(parts[1])
      );

      const claim = payload['EmployeeId'];

      return claim ? Number(claim) : null;

    } catch {
      return null;
    }
  }
}