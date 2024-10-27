import { HandlerCaller } from 'lib/ericchase/Design Pattern/Handler.js';
import { Defer } from 'lib/ericchase/Utility/Defer.js';
import { Sleep } from 'lib/ericchase/Utility/Sleep.js';

export class WebSocketClient {
  $address: string;
  $closed = Defer();
  $socket?: WebSocket;
  constructor({
    address,
    auto_reconnect = false,
    auto_reconnect_delay = 5000,
  }: {
    address: string;
    auto_reconnect?: boolean;
    auto_reconnect_delay?: number;
  }) {
    this.$address = address;
    if (auto_reconnect === true) {
      const reconnect = async () => {
        if (this.$socket === undefined) {
          await this.connect();
          setTimeout(reconnect, auto_reconnect_delay);
        }
      };
      this.onClose(() => {
        setTimeout(reconnect, auto_reconnect_delay);
      });
      this.onError(() => {
        setTimeout(reconnect, auto_reconnect_delay);
      });
    }
  }
  async connect() {
    await this.disconnect();
    this.$closed = Defer();
    this.$socket = new WebSocket(this.$address);
    this.$socket?.addEventListener('error', async (event) => {
      this.$onerror.call(event);
      this.$socket = undefined;
      // this.$closed.resolve();
    });
    this.$socket?.addEventListener('open', async (event) => {
      this.$socket?.addEventListener('close', async (event) => {
        this.$onclose.call(event);
        this.$socket = undefined;
        this.$closed.resolve();
      });
      this.$onopen.call(event);
    });
    this.$socket?.addEventListener('message', (event) => {
      this.$onmessage.call(event);
    });
  }
  disconnect(timeout = 5000) {
    if (this.$socket !== undefined) {
      this.$socket?.close();
      Sleep(timeout).then(() => {
        this.$closed.resolve();
      });
      return this.$closed.promise;
    }
  }
  onClose(fn: (event: CloseEvent) => void) {
    this.$onclose.add(fn);
  }
  onError(fn: (event: Event) => void) {
    this.$onerror.add(fn);
  }
  onMessage(fn: (evt: MessageEvent<any>) => void) {
    this.$onmessage.add(fn);
  }
  onOpen(fn: (event: Event) => void) {
    this.$onopen.add(fn);
  }
  send(data: string | ArrayBufferLike | Blob | ArrayBufferView) {
    this.$socket?.send(data);
  }
  $onclose = new HandlerCaller<CloseEvent>();
  $onerror = new HandlerCaller<Event>();
  $onmessage = new HandlerCaller<MessageEvent>();
  $onopen = new HandlerCaller<Event>();
}
