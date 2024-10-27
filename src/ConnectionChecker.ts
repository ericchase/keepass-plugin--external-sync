export class ConnectionChecker {
  $alive = true;
  $timer?: Timer;
  constructor(
    public $interval: number,
    public SendMessage: (message: string) => void,
    public Exit: () => void,
  ) {
    this.$ResetTimer();
  }
  CleanUp() {
    clearTimeout(this.$timer);
    this.$alive = false;
    this.$timer = undefined;
    this.SendMessage = (_ignore: string) => {};
    this.Exit = () => {};
  }
  ProcessPing() {
    this.$alive = true;
    this.SendMessage('pong');
    this.$ResetTimer();
  }
  ProcessPong() {
    this.$alive = true;
  }
  $CheckConnection() {
    if (this.$alive === true) {
      this.$alive = false;
      this.SendMessage('ping');
      this.$ResetTimer();
    } else {
      this.Exit();
      this.CleanUp();
    }
  }
  $ResetTimer() {
    clearTimeout(this.$timer);
    this.$timer = setTimeout(() => {
      this.$CheckConnection();
    }, this.$interval);
  }
}
