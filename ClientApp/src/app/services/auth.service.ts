import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http'; // For backend API calls; import if using

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private _isAuthenticated = new BehaviorSubject<boolean>(this.checkInitialAuth());
  private _userRole = new BehaviorSubject<string>(this.getInitialRole());
  private _username = new BehaviorSubject<string>(this.getInitialUsername());

  isAuthenticated$: Observable<boolean> = this._isAuthenticated.asObservable();
  userRole$: Observable<string> = this._userRole.asObservable();
  username$: Observable<string> = this._username.asObservable();

  constructor(private http: HttpClient) {} // Inject HttpClient if making API calls

  // Example login method (adapt to your backend)
  login(credentials: { email: string; password: string }): Observable<any> {
    // Replace with your actual login API endpoint, e.g., '/api/account/login'
    return this.http.post('/api/account/login', credentials).pipe(
      // tap(response => {
      //   localStorage.setItem('token', response.token); // Store JWT or session token
      //   this._isAuthenticated.next(true);
      //   this._userRole.next(response.role); // e.g., 'Teacher'
      //   this._username.next(response.username);
      // })
    );
  }

  // Example logout method
  logout(): void {
    // Call backend logout if needed, e.g., this.http.post('/api/account/logout', {});
    localStorage.removeItem('token');
    this._isAuthenticated.next(false);
    this._userRole.next('');
    this._username.next('');
  }

  // Check if user is authenticated (e.g., on app init)
  private checkInitialAuth(): boolean {
    return !!localStorage.getItem('token'); // Or check cookie/session
  }

  private getInitialRole(): string {
    // Fetch from token or backend; stub for now
    return ''; // e.g., parse from JWT
  }

  private getInitialUsername(): string {
    // Fetch from token or backend; stub for now
    return ''; // e.g., parse from JWT
  }

  // Additional methods, e.g., to check roles
  isTeacher(): boolean {
    return this._userRole.value === 'Teacher';
  }
}