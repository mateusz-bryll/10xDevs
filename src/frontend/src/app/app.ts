import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { Menubar } from 'primeng/menubar';

@Component({
  selector: 'app-root',
  imports: [ Menubar, RouterOutlet ],
  template: `
  <div class="card">
    <p-menubar [model]="menuItems">
    </p-menubar>
  </div>
  <router-outlet></router-outlet>
  `,
})
export class App implements OnInit {
  public menuItems: MenuItem[] | undefined;

  public ngOnInit(): void {
    this.menuItems = [
      {
        label: 'Home',
        icon: 'pi pi-home',
        routerLink: '/'
      }
    ];
  }
}
