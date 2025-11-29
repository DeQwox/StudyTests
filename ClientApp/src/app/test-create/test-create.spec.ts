import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TestCreate } from './test-create';

describe('TestCreate', () => {
  let component: TestCreate;
  let fixture: ComponentFixture<TestCreate>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestCreate]
    })
    .compileComponents();

    fixture = TestBed.createComponent(TestCreate);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
