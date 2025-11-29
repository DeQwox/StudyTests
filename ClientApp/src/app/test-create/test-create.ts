import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { TestService } from '../test.service';

@Component({
  selector: 'app-test-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './test-create.html'
})
export class TestCreateComponent {
  form: FormGroup;
  minDate: string;

  constructor(private fb: FormBuilder, private testService: TestService, private router: Router) {
    // Set min date logic [cite: 26, 27]
    const now = new Date();
    this.minDate = now.toISOString().slice(0, 16);

    this.form = this.fb.group({
      name: ['', Validators.required],
      description: ['', Validators.required],
      password: [''], // [cite: 24]
      validUntil: ['', this.futureDateValidator], // Custom validator for [cite: 28]
      questions: this.fb.array([])
    });
  }

  get questions() {
    return this.form.get('questions') as FormArray;
  }

  // Logic to add a question card [cite: 25, 30]
  addQuestion() {
    const questionGroup = this.fb.group({
      description: ['', Validators.required],
      score: [1, Validators.min(0.1)],
      answers: this.fb.array([this.createAnswer(), this.createAnswer()]), // Start with 2 answers
      correctAnswerIndex: [0] // Tracks the index of the correct radio button
    });
    this.questions.push(questionGroup);
  }

  removeQuestion(index: number) { // [cite: 33]
    this.questions.removeAt(index);
  }

  // Answer logic within a question
  createAnswer(): FormGroup {
    return this.fb.group({
      text: ['', Validators.required]
    });
  }

  getAnswers(questionIndex: number): FormArray {
    return this.questions.at(questionIndex).get('answers') as FormArray;
  }

  addAnswer(questionIndex: number) { // [cite: 32]
    this.getAnswers(questionIndex).push(this.createAnswer());
  }

  submit() {
    if (this.form.invalid) return;

    const formValue = this.form.value;

    // Map form data to the API DTO structure
    const payload = {
      name: formValue.name,
      description: formValue.description,
      password: formValue.password,
      validUntil: formValue.validUntil || null, // Handle empty date string
      questions: formValue.questions.map((q: any) => ({
        description: q.description,
        score: q.score,
        correctAnswerIndex: q.correctAnswerIndex,
        answers: q.answers.map((a: any) => a.text)
      }))
    };

    this.testService.createTest(payload).subscribe({
      next: () => {
        this.router.navigate(['/tests']);
      },
      error: (err) => {
        console.error('Error creating test', err);
        // Optional: Add alert('Error creating test');
      }
    });
  }

  // Custom validator for future dates [cite: 28]
  futureDateValidator(control: any) {
    if (!control.value) return null;
    return new Date(control.value) <= new Date() ? { pastDate: true } : null;
  }
}