import { Component, signal } from '@angular/core';
import { MatToolbar } from '@angular/material/toolbar';
import { MatIconButton } from '@angular/material/button';
import { MatFormField, MatOption, MatSelect, MatSelectTrigger } from '@angular/material/select';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatListItem, MatListItemIcon, MatListItemTitle, MatNavList } from '@angular/material/list';

@Component({
  imports: [
    MatToolbar,
    MatIconButton,
    MatSelect,
    MatOption,
    MatFormField,
    MatSelectTrigger,
    RouterOutlet,
    MatNavList,
    MatListItem,
    MatListItemTitle,
    MatListItemIcon,
    RouterLink,
    RouterLinkActive,
  ],
  selector: 'app-main',
  styleUrl: './main.css',
  templateUrl: './main.html',
})
export default class Main {
  isOpenedNavBar = signal(true);

  toggleButton(): void {
    this.isOpenedNavBar.update((val) => !val);
  }
}
