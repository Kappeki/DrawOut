import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { User } from '../shared/models/user';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DrawOutAPIService } from '../services/draw-out-api.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent implements OnInit {
  nicknameText: string = '';
  selectedIcon: string | ArrayBuffer | null = null;

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      this.selectedIcon = reader.result;
    };
  }
  
  constructor(private apiService: DrawOutAPIService, private router: Router) { }

  ngOnInit(): void {
    this.apiService.getSession().subscribe({
      next: (session: any) => {
        if (session) {
          this.nicknameText = session.nickname;
          this.selectedIcon = session.icon;
        }
      },
      error: (error: any) => {
        console.error(error);
      },
      complete: () => {
        console.log('Session retrieval completed');
      }
    });
  }

  onPressPlay() {
    const userPrefs = {
      nickname: this.nicknameText,
      icon: this.selectedIcon
    }
    this.router.navigate(['/rooms']);
    this.apiService.createUser(userPrefs).subscribe({
      next: (response: any) => {
        this.router.navigate(['/rooms']);
        console.log('User created');
      },
      error: (error: any) => {
        console.error(error);
      },
      complete: () => {
        console.log('User creation completed');
      }
    });
  }

  onPressCreateRoom() {
    // this.router.navigate(['/rooms']);
  }
}
