import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { User } from '../../models/user';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.css'
})
export class UserListComponent {
  @Input() users: User[] = [];
  @Input() team: string[] = [];
  @Input() className: 'Spectators' | 'Red' | 'Blue' = 'Spectators';
  @Input() currentTeam: string | null = null;  // Receive currentTeam from parent
  @Output() teamChange = new EventEmitter<{ oldTeam: string | null, newTeam: string }>();

  joinTeam() {
    const oldTeam = this.currentTeam;
    const newTeam = this.className;
    this.teamChange.emit({ oldTeam, newTeam });
  }

  get teamUsers(): User[] {
    if (this.className === 'Spectators') {
      return this.users.filter(user => !this.team.includes(user.nickname));
    } else {
      return this.users.filter(user => this.team.includes(user.nickname));
    }
  }
}
