export class BaseDto<T> {
  eventType: string;

  constructor(init?: Partial<any>) {
    this.eventType = this.constructor.name;
    Object.assign(this, init);
  }
}

export class ClientWantsToSignIn extends BaseDto<ClientWantsToSignIn> {
  userName?: string;

  constructor(userName: string) {
    super();
    this.userName = userName;
  }
}

export class ClientSignedIn extends BaseDto<ClientSignedIn> {
  message: string;
  username: string;

  constructor(message: string, username: string) {
    super();
    this.message = message;
    this.username = username;
  }
}

export class ClientTurn extends BaseDto<ClientTurn> {
  clicked: boolean;

  constructor(clicked: boolean) {
    super();
    this.clicked = clicked;
  }
}

export class ClientTurnStarted extends BaseDto<ClientTurnStarted> {
  playerId: string;
  username: string;

  constructor(playerId: string, username: string) {
    super();
    this.playerId = playerId;
    this.username = username;
  }
}

export class ClientCurrentStatus extends BaseDto<ClientCurrentStatus> {
  playersStatus: PlayerStatus[];

  constructor(playersStatus: PlayerStatus[]) {
    super();
    this.playersStatus = playersStatus;
  }
}

export class PlayerStatus {
  playerId: string;
  username: string;
  totalTime: string;

  constructor(playerId: string, username: string, totalTime: string) {
    this.playerId = playerId;
    this.username = username;
    this.totalTime = totalTime;
  }
}

export class NotYourTurn extends BaseDto<NotYourTurn> {
  message: string;

  constructor(message: string) {
    super();
    this.message = message;
  }
}

export class ClientWon extends BaseDto<ClientWon> {
  message: string;

  constructor(message: string) {
    super();
    this.message = message;
  }
}


export class ClientReachedMaxTime extends BaseDto<ClientReachedMaxTime> {
  message: string;

  constructor(message: string) {
    super();
    this.message = message;
  }
}

export class ClientError extends BaseDto<ClientError> {
  errorMessage: string;

  constructor(errorMessage: string) {
    super();
    this.errorMessage = errorMessage;
  }
}

export class ClientMustBeSignedIn extends BaseDto<ClientMustBeSignedIn> {
  errorMessage: string;

  constructor(errorMessage: string) {
    super();
    this.errorMessage = errorMessage;
  }
}
