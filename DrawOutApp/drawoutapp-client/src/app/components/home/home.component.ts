import { Component, OnInit, Signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DrawOutAPIService } from '../../services/draw-out-api.service';

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
  isModalOpen = false;
  roomName = '';
  password: string | null = null;
  sessionExists = false;

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      this.selectedIcon = reader.result;
    };
  }

  //nedostaje chat component 

  //home compoentn i room component bind preko pass
  //room list component i room component bind preko roomid i pass
  constructor(private apiService: DrawOutAPIService, private router: Router) { }

  ngOnInit(): void {
    this.apiService.getSession().subscribe({
      next: (session: any) => {
        if (session) {
          this.nicknameText = session.nickname;
          this.selectedIcon = session.icon;
          this.sessionExists = true;
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
    if (this.sessionExists) {
      this.apiService.updateUserPrefs(userPrefs).subscribe({
        next: (response: any) => {
          this.router.navigate(['/rooms']);
          console.log('User updated');
        },
        error: (error: any) => {
          console.error(error);
        },
        complete: () => {
          console.log('User update completed');
        }
      });
    } else {
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
  }

  openRoomModal() {
    this.isModalOpen = true;
  }

  closeRoomModal() {
    this.isModalOpen = false;
  }

  createRoom() {
    // name check if exists
    this.apiService.createRoom(this.roomName, this.password!).subscribe({
      next: (response: any) => {
        this.router.navigate(['/room/by-url', response.roomUrl]);
        console.log(`Room created with ${response.roomUrl}`);
      },
      error: (error: any) => {
        console.error(error);
      },
      complete: () => {
        console.log('Room creation completed');
      }
    });

    // Close the modal
    this.closeRoomModal();
  }

}
