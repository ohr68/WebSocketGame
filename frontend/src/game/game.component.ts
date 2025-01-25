import {Component, Input, OnDestroy, OnInit} from "@angular/core";
import {WebSocketService} from '../services/websocket.service';
import {
  BaseDto,
  ClientCurrentStatus, ClientError,
  ClientReachedMaxTime,
  ClientTurn,
  ClientTurnStarted, ClientWon,
  NotYourTurn
} from '../baseDto';
import {RawHtmlComponent} from '../raw-html/raw-html.component';
import {NgClass, NgForOf, NgIf} from '@angular/common';
import {FormsModule} from '@angular/forms';

@Component({
  selector: "game",
  templateUrl: "./game.component.html",
  imports: [
    RawHtmlComponent,
    NgForOf,
    FormsModule,
    NgClass,
    NgIf
  ],
  styleUrls: ["./game.component.css"]
})

export class GameComponent implements OnInit, OnDestroy {
  @Input() currentUser!: string;

  currentPlayerTurn = '';
  started = false;
  isMyTurn = false;
  clicked = false;
  currentStatus!: string [];

  constructor(private webSocketService: WebSocketService) {
    this.currentStatus = [];
  }

  ngOnInit(): void {
    this.webSocketService.messages.subscribe((event) => {

      const messageFromServer = event as BaseDto<any>;
      console.log(messageFromServer);

      this.handleMessageFromServer(messageFromServer, event);
    });
  }

  sendClick() {
    const clickEvent = new ClientTurn(true);

    this.webSocketService.sendMessage(clickEvent);

    this.clicked = true;
  }

  ngOnDestroy(): void {
    this.webSocketService.close();
  }

  handleMessageFromServer(messageFromServer: any, event: any) {
    switch (messageFromServer.eventType) {
      case "ClientCurrentStatus":
        this.handleClientCurrentStatus(event as ClientCurrentStatus);
        break;

      case "ClientTurnStarted":
        this.handleClientTurnStarted(event as ClientTurnStarted);
        break;

      case "NotYourTurn":
        this.handleNotYourTurn(event as NotYourTurn);
        break;

      case "ClientReachedMaxTime":
        this.handleClientReachedMaxTime(event as ClientReachedMaxTime);
        break;

      case "ClientError":
        this.handleClientError(event as ClientError);
        break;

      case "ClientWon":
        this.handleClientWon(event as ClientWon);
        break;

      default:
        console.warn("Unknown event type:", messageFromServer.eventType);
    }
  }

  private handleClientCurrentStatus(currentStatus: ClientCurrentStatus): void {
    this.started = true;
    this.currentStatus = [];

    this.currentStatus = currentStatus.playersStatus.map(playerStatus =>
      `<div class="stat">
       <div class="stat-title">${playerStatus.username}</div>
       <div class="stat-value">${playerStatus.totalTime}</div>
       <div class="stat-desc"></div>
     </div>`
    );
  }

  private handleClientTurnStarted(turnStarted: ClientTurnStarted): void {
    if (turnStarted.username !== this.currentUser) {
      this.currentPlayerTurn = `Turno de '${turnStarted.username}'`;
      this.isMyTurn = false;
      this.clicked = true;
    } else {
      this.currentPlayerTurn = "Seu turno";
      this.isMyTurn = true;
      this.clicked = false;
    }
  }

  private handleNotYourTurn(notYourTurn: NotYourTurn): void {
    alert(notYourTurn.message);
    this.clicked = true;
  }

  private handleClientReachedMaxTime(reachedMaxTime: ClientReachedMaxTime): void {
    alert(reachedMaxTime.message);
    this.finishGame();
  }

  private handleClientError(error: ClientError): void {
    alert(error.errorMessage);
    this.clicked = false;
  }

  private handleClientWon(won: ClientWon): void {
    alert(won.message);
    this.finishGame();
  }

  private finishGame(): void {
    this.started = false;
    this.clicked = true;
    this.currentStatus = [];
  }
}
