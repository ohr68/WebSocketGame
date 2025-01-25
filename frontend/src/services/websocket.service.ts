import {Injectable} from "@angular/core";
import {Observable, Subject} from 'rxjs';
import {BaseDto, ClientWantsToSignIn} from '../baseDto';

@Injectable({
  providedIn: 'root',
})

export class WebSocketService {
  private socket!: WebSocket;
  private messages$: Subject<any> = new Subject<any>();

  connect(url: string = "ws://localhost:8181/"): void {
    if (!this.socket || this.socket.readyState === WebSocket.CLOSED) {
      this.socket = new WebSocket(url);

      this.socket.onmessage = (message) => {
        this.messages$.next(JSON.parse(message.data));
      };

      this.socket.onerror = (error) => {
        console.error('WebSocket Error:', error);
      };

      this.socket.onclose = (event) => {
        console.error('WebSocket connection closed:', event);
      };
    }
  }

  signIn(username: string): void {
    let signInRequest = new ClientWantsToSignIn(username);

    this.sendMessage(signInRequest);
  }

  sendMessage(message: any): void {
    if (this.socket && this.socket.readyState == WebSocket.OPEN) {
      this.socket.send(JSON.stringify(message));
    } else {
      console.error('WebSocket is not open');
    }
  }

  close(): void {
    if (this.socket) {
      this.socket.close();
    }
  }

  get messages(): Observable<any> {
    return this.messages$.asObservable();
  }
}
