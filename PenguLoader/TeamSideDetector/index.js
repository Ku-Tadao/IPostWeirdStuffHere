window.addEventListener('load', () => {
  let isInChampSelect = false;
  let currentTeam = null;
  let notification = null;
  
  const style = document.createElement('style');
  style.textContent = `
    .team-side-notification {
      position: fixed;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      padding: 20px 40px;
      border-radius: 8px;
      font-size: 24px;
      font-weight: bold;
      color: white;
      z-index: 9999;
      opacity: 0;
      transition: opacity 0.2s;
      pointer-events: none;
    }
    
    .team-side-notification.blue {
      background: rgba(0, 48, 163, 0.9);
      box-shadow: 0 0 20px rgba(0, 148, 255, 0.5);
    }
    
    .team-side-notification.red {
      background: rgba(163, 0, 0, 0.9);
      box-shadow: 0 0 20px rgba(255, 0, 0, 0.5);
    }
    
    .team-side-notification.visible {
      opacity: 1;
    }
  `;
  document.head.appendChild(style);

  const createNotification = () => {
    notification = document.createElement('div');
    notification.className = 'team-side-notification';
    document.body.appendChild(notification);
    return notification;
  };

  const showNotification = (team) => {
    if (!notification) notification = createNotification();
    notification.className = `team-side-notification ${team.toLowerCase()}`;
    notification.textContent = `${team} Side`;
    notification.classList.add('visible');
  };

  const hideNotification = () => {
    if (notification) {
      notification.classList.remove('visible');
    }
  };

  const determineTeamSide = (data) => {
    const localPlayer = data.myTeam.find(player => player.cellId === data.localPlayerCellId);
    return localPlayer?.team === 1 ? 'BLUE' : 'RED';
  };

  const checkChampSelect = async () => {
    try {
      const response = await fetch('./lol-champ-select/v1/session');
      if (response.ok) {
        const data = await response.json();
        currentTeam = determineTeamSide(data);
      }
    } catch (error) {
      currentTeam = null;
    }
  };

  const checkGamePhase = async () => {
    try {
      const response = await fetch('./lol-gameflow/v1/gameflow-phase');
      if (!response.ok) return;
      
      const phase = await response.text();
      const wasInChampSelect = isInChampSelect;
      isInChampSelect = phase === '"ChampSelect"';

      if (isInChampSelect && !wasInChampSelect) {
        await checkChampSelect();
      } else if (!isInChampSelect && wasInChampSelect) {
        currentTeam = null;
        hideNotification();
      }
    } catch (error) {
      isInChampSelect = false;
      currentTeam = null;
      hideNotification();
    }
  };

  const handleKeyState = (event) => {
    if (event.ctrlKey && event.shiftKey && isInChampSelect && currentTeam) {
      showNotification(currentTeam);
    } else {
      hideNotification();
    }
  };

  document.addEventListener('keydown', handleKeyState);
  document.addEventListener('keyup', handleKeyState);

  checkGamePhase();
  
  const ws = new WebSocket('wss://127.0.0.1:34243', 'wamp');
  
  ws.onopen = () => {
    ws.send(JSON.stringify([5, 'OnJsonApiEvent_lol-gameflow_v1_gameflow-phase']));
  };

  ws.onmessage = async (message) => {
    try {
      const data = JSON.parse(message.data);
      if (data[2]?.uri === '/lol-gameflow/v1/gameflow-phase') {
        const phase = data[2].data;
        const wasInChampSelect = isInChampSelect;
        isInChampSelect = phase === 'ChampSelect';

        if (isInChampSelect && !wasInChampSelect) {
          await checkChampSelect();
        } else if (!isInChampSelect && wasInChampSelect) {
          currentTeam = null;
          hideNotification();
        }
      }
    } catch (error) {
      console.error('Error processing WebSocket message:', error);
    }
  };

  setInterval(checkGamePhase, 1000);
});
