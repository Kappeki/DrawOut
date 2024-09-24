import { Component, OnInit, Signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { DrawOutAPIService } from '../../services/draw-out-api.service';
import { SessionService } from '../../services/session.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css', '../../app.component.css']
})
export class HomeComponent implements OnInit {
  nicknameText: string = '';
  selectedIcon: string | ArrayBuffer | null = null;
  isModalOpen = false;
  roomName = '';
  password: string | null = null;
  sessionExists = false;
  passwordInputType: string = 'password';

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    const reader = new FileReader();
    reader.readAsDataURL(file);
    reader.onload = () => {
      this.selectedIcon = reader.result;
    };
  }

  constructor(private apiService: DrawOutAPIService, private router: Router, private sessionService: SessionService) { }

  ngOnInit(): void {
    this.apiService.getSession().subscribe({
      next: (session: any) => {
        if (session) {
          this.nicknameText = session.nickname;
          this.selectedIcon = session.icon;
          this.sessionExists = true;
          this.sessionService.setSessionId(session._sessionKey);
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
          this.sessionService.setSessionId(response._sessionKey);
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

  closeModalOnOutsideClick(event: MouseEvent) {
    if ((<HTMLElement>event.target).id === 'modal') {
        this.closeRoomModal();
    }
  }

  closeRoomModal() {
    this.isModalOpen = false;
  }

  createRoom() {
    const roomNameInput = document.getElementById('roomName');
    
    if (this.roomName === '') {
        roomNameInput?.classList.add('input-error');
        
        const snackbar = document.getElementById('snackbar');
        snackbar!.innerText = 'Room name must be entered!';
        snackbar!.className = "show";

        setTimeout(() => {
            roomNameInput?.classList.remove('input-error');
            snackbar!.className = snackbar!.className.replace("show", "");
        }, 3000);
        
        return;
    }

    let passwordToSend = this.password?.trim();
    if (passwordToSend === '') {
        passwordToSend = undefined; //ovo je kako ne bi sifra bila prazan string nakon sto korisnik napise nesto za sifru pa nakon toga obrise
    }

    this.apiService.createRoom(this.roomName, passwordToSend).subscribe({
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
    this.closeRoomModal();
  }

  togglePasswordVisibility() {
    this.passwordInputType = this.passwordInputType === 'password' ? 'text' : 'password';
  }

  randomizeIcon() {
    this.apiService.getRandomIcon().subscribe({
      next: (response: string) => {
        this.selectedIcon = response;
      },
      error: (error: any) => {
        console.error(error);
      },
      complete: () => {
        console.log('Icon randomization completed');
      }
    });
  }

  randomizeNickname() {
    this.apiService.getRandomNickname().subscribe({
      next: (response: string) => {
        this.nicknameText = response;
      },
      error: (error: any) => {
        console.error(error);
      },
      complete: () => {
        console.log('Nickname randomization completed');
      }
    });
  }
}
