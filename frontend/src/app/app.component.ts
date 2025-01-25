import {Component, OnInit} from '@angular/core';
import {RouterOutlet} from '@angular/router';
import {FormControl, ReactiveFormsModule} from '@angular/forms';
import {
  BaseDto, ClientSignedIn,
} from '../baseDto';
import {WebSocketService} from '../services/websocket.service';
import {NgIf} from '@angular/common';
import {GameComponent} from '../game/game.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, ReactiveFormsModule, NgIf, GameComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})

export class AppComponent implements OnInit {
  title = 'frontend';

  messages: string[] = [];
  isAuthenticated: boolean = false;
  loggedUser: string = '';

  username: FormControl<string | null> = new FormControl('');

  constructor(private webSocketService: WebSocketService) {
  }

  ngOnInit() {
    this.webSocketService.connect();

    this.webSocketService.messages.subscribe((event) => {
      const messageFromServer = event as BaseDto<any>;

      if (messageFromServer.eventType == "ClientSignedIn") {
        this.isAuthenticated = true;
      }

      this.messages.push(JSON.stringify(event.message));
    });
  }

  signIn() {
    this.webSocketService!.signIn(this.username.value!);
  }
}
