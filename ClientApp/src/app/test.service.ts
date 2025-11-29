import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TestSummary, TestDetail, TestCreate } from './test.models';

@Injectable({ providedIn: 'root' })
export class TestService {
  private apiUrl = 'https://localhost:5001/api/tests';

  constructor(private http: HttpClient) {}

  getTests(): Observable<TestSummary[]> {
    return this.http.get<TestSummary[]>(this.apiUrl);
  }

  getTest(id: number): Observable<TestDetail> {
    return this.http.get<TestDetail>(`${this.apiUrl}/${id}`);
  }

  createTest(test: TestCreate): Observable<any> {
    return this.http.post(this.apiUrl, test);
  }

  deleteTest(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}