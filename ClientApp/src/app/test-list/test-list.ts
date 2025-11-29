import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TestService } from '../test.service';
import { TestSummary } from '../test.models';

@Component({
  selector: 'app-test-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './test-list.html',
  styleUrls: ['./test-list.scss']
})
export class TestListComponent implements OnInit {
  tests: TestSummary[] = [];
  loading = true;
  error = '';

  constructor(
    private testService: TestService,
    private cdr: ChangeDetectorRef // 2. Inject it here
  ) {}

  ngOnInit(): void {
    this.testService.getTests().subscribe({
      next: (data) => {
        this.tests = data || []; 
        this.loading = false;
        
        console.log('Data loaded:', this.tests);
        
        this.cdr.detectChanges(); 
      },
      error: (err) => {
        console.error('API Error:', err);
        this.error = 'Не вдалося завантажити тести. Перевірте з’єднання з сервером.';
        this.loading = false;
        
        this.cdr.detectChanges(); 
      }
    });
  }
}