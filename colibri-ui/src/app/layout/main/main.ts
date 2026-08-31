import { Component } from '@angular/core';
import { MatToolbar } from '@angular/material/toolbar';
import { MatIconButton } from '@angular/material/button';
import { MatFormField, MatOption, MatSelect, MatSelectTrigger } from '@angular/material/select';
import { MatSidenav, MatSidenavContainer, MatSidenavContent } from '@angular/material/sidenav';
import { RouterOutlet } from '@angular/router';
import { MatListItem, MatListItemIcon, MatListItemTitle, MatNavList } from '@angular/material/list';

@Component({
  imports: [
    MatToolbar,
    MatIconButton,
    MatSelect,
    MatOption,
    MatFormField,
    MatSelectTrigger,
    MatSidenavContainer,
    MatSidenav,
    MatSidenavContent,
    RouterOutlet,
    MatNavList,
    MatListItem,
    MatListItemTitle,
    MatListItemIcon,
  ],
  selector: 'app-main',
  styleUrl: './main.css',
  templateUrl: './main.html',
})
export default class Main {}
