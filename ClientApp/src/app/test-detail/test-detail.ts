import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { TestService } from '../test.service';
import { TestDetail } from '../test.models';

@Component({
  selector: 'app-test-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './test-detail.html'
})
export class TestDetailComponent implements OnInit {
  test: TestDetail | null = null;
  loading = true;
  error = '';

  constructor(
    private route: ActivatedRoute,
    private testService: TestService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    
    this.testService.getTest(id).subscribe({
      next: (data) => {
        this.test = data;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        console.error('API Error:', err);
        this.error = 'Не вдалося завантажити деталі тесту.';
        this.loading = false;
        this.cdr.detectChanges();
      }
    });
  }

  deleteTest() {
    if (confirm('Ви впевнені, що хочете видалити цей тест? Це також видалить усі результати учнів.') && this.test) {
      this.testService.deleteTest(this.test.id).subscribe(() => {
        this.router.navigate(['/tests']);
      });
    }
  }
}