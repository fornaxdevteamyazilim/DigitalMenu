// Kasa / mutfak: garson çağrısı ve hesap isteği ses + bildirim + sekme başlığı
window.staffAlerts = {
  audioContext: null,
  titleFlashTimer: null,
  originalTitle: document.title,
  activeWaiterAlerts: 0,
  activeBillAlerts: 0,
  notificationsByTag: {},

  async unlock() {
    const AudioCtx = window.AudioContext || window.webkitAudioContext;
    if (!AudioCtx) return false;

    if (!this.audioContext) {
      this.audioContext = new AudioCtx();
    }

    if (this.audioContext.state === 'suspended') {
      await this.audioContext.resume();
    }

    return this.audioContext.state === 'running';
  },

  isUnlocked() {
    return this.audioContext != null && this.audioContext.state === 'running';
  },

  async requestNotificationPermission() {
    if (!('Notification' in window)) return false;
    if (Notification.permission === 'granted') return true;
    if (Notification.permission === 'denied') return false;
    const result = await Notification.requestPermission();
    return result === 'granted';
  },

  async playWaiterAlert() {
    await this._playPattern([
      { freq: 880, duration: 0.18, gap: 0 },
      { freq: 1100, duration: 0.18, gap: 0.1 },
      { freq: 880, duration: 0.18, gap: 0.35 },
      { freq: 1100, duration: 0.18, gap: 0.1 },
      { freq: 1320, duration: 0.28, gap: 0.35 },
    ]);
  },

  async playBillAlert() {
    await this._playPattern([
      { freq: 520, duration: 0.22, gap: 0 },
      { freq: 660, duration: 0.22, gap: 0.12 },
      { freq: 780, duration: 0.3, gap: 0.4 },
    ]);
  },

  async _playPattern(pattern) {
    try {
      await this.unlock();
      if (!this.audioContext) return;

      let delay = 0;
      for (const note of pattern) {
        this._scheduleBeep(this.audioContext, note.freq, note.duration, delay);
        delay += note.duration + note.gap;
      }
    } catch (e) {
      console.warn('staffAlerts._playPattern', e);
    }
  },

  _scheduleBeep(ctx, frequency, duration, startDelaySec) {
    const startAt = ctx.currentTime + startDelaySec;
    const osc = ctx.createOscillator();
    const gain = ctx.createGain();

    osc.type = 'square';
    osc.frequency.setValueAtTime(frequency, startAt);
    gain.gain.setValueAtTime(0.0001, startAt);
    gain.gain.exponentialRampToValueAtTime(0.35, startAt + 0.02);
    gain.gain.exponentialRampToValueAtTime(0.0001, startAt + duration);

    osc.connect(gain);
    gain.connect(ctx.destination);
    osc.start(startAt);
    osc.stop(startAt + duration + 0.02);
  },

  showNotification(title, body, tag) {
    if (!('Notification' in window) || Notification.permission !== 'granted') return;
    const notificationTag = tag || 'staff-alert';
    try {
      const notification = new Notification(title, {
        body,
        icon: '/favicon.png',
        tag: notificationTag,
        renotify: true,
        requireInteraction: true,
      });
      this.notificationsByTag[notificationTag] = notification;
      notification.onclick = () => {
        window.focus();
        notification.close();
      };
    } catch (e) {
      console.warn('staffAlerts.showNotification', e);
    }
  },

  closeNotification(tag) {
    const n = this.notificationsByTag[tag];
    if (n) {
      n.close();
      delete this.notificationsByTag[tag];
    }
  },

  syncWaiterAlertCount(count) {
    this.activeWaiterAlerts = Math.max(0, count);
    this.refreshTitleFlash();
  },

  syncBillAlertCount(count) {
    this.activeBillAlerts = Math.max(0, count);
    this.refreshTitleFlash();
  },

  refreshTitleFlash() {
    this.stopTitleFlash();
    if (this.activeWaiterAlerts > 0) {
      this.startTitleFlash('🔔 Garson çağrısı');
    } else if (this.activeBillAlerts > 0) {
      this.startTitleFlash('💳 Hesap isteği');
    }
  },

  startTitleFlash(alertTitle) {
    this.originalTitle = document.title;
    let showAlert = true;
    this.titleFlashTimer = setInterval(() => {
      document.title = showAlert ? alertTitle : this.originalTitle;
      showAlert = !showAlert;
    }, 700);
  },

  stopTitleFlash() {
    if (this.titleFlashTimer) {
      clearInterval(this.titleFlashTimer);
      this.titleFlashTimer = null;
    }
    document.title = this.originalTitle;
  },
};

(function setupStaffAlertUnlock() {
  const onFirstInteraction = () => {
    window.staffAlerts.unlock();
    window.staffAlerts.requestNotificationPermission();
  };
  document.addEventListener('pointerdown', onFirstInteraction, { once: true, passive: true });
  document.addEventListener('keydown', onFirstInteraction, { once: true });
})();
