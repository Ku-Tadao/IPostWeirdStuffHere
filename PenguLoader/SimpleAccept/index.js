export function init({ socket }) {
  let hasAccepted = false;
  let normalRetryCount = 0;
  let aggressiveRetryCount = 0;
  const MAX_RETRIES = 3;
  const RETRY_DELAY = 100;

  const resetState = () => {
    hasAccepted = false;
    normalRetryCount = 0;
    aggressiveRetryCount = 0;
  };

  const attemptAcceptance = async () => {
    if (hasAccepted) return;

    if (normalRetryCount < MAX_RETRIES) {
      try {
        
        const response = await fetch('/lol-matchmaking/v1/ready-check/accept', {
          method: 'POST',
        });

        if (response.ok) {
          hasAccepted = true;
          return true;
        } else {
          throw new Error(`HTTP ${response.status}`);
        }
      } catch (error) {
        console.warn(`❌ Normal attempt ${normalRetryCount + 1} failed:`, error);
        normalRetryCount++;
        
        if (normalRetryCount < MAX_RETRIES) {
          const delay = RETRY_DELAY * Math.pow(2, normalRetryCount - 1) + Math.random() * 50;
          await new Promise(resolve => setTimeout(resolve, delay));
          return attemptAcceptance();
        }
      }
    }

    if (aggressiveRetryCount < MAX_RETRIES) {
      try {
        
        if (typeof window !== 'undefined') {
          window.focus();
          await new Promise(resolve => setTimeout(resolve, 100));
        }
        
        const response = await fetch('/lol-matchmaking/v1/ready-check/accept', {
          method: 'POST',
        });

        if (response.ok) {
          hasAccepted = true;
          return true;
        } else {
          throw new Error(`HTTP ${response.status}`);
        }
      } catch (error) {
        console.warn(`❌ Aggressive attempt ${aggressiveRetryCount + 1} failed:`, error);
        aggressiveRetryCount++;
        
        if (aggressiveRetryCount < MAX_RETRIES) {
          const delay = RETRY_DELAY * Math.pow(2, aggressiveRetryCount - 1) + Math.random() * 50;
          await new Promise(resolve => setTimeout(resolve, delay));
          return attemptAcceptance();
        }
      }
    }

    console.error('🚫 All acceptance attempts failed');
    return false;
  };

  const readyCheckObserver = socket.observe('/lol-matchmaking/v1/ready-check', async (message) => {
    
    if (message.eventType === 'Delete') {
      resetState();
      return;
    }

    if (message.eventType === 'Update' && message.data) {
      const { playerResponse, state } = message.data;

      if (state === 'InProgress' && !hasAccepted && playerResponse === 'None') {
        await new Promise(resolve => setTimeout(resolve, 50));
        await attemptAcceptance();
      } else if (state !== 'InProgress' && hasAccepted) {
        resetState();
      }
    }
  });

  let pollInterval = null;
  
  const startPolling = () => {
    if (pollInterval || hasAccepted) return;
    
    pollInterval = setInterval(async () => {
      if (hasAccepted) {
        stopPolling();
        return;
      }
      
      try {
        const response = await fetch('/lol-matchmaking/v1/ready-check');
        if (response.ok) {
          const data = await response.json();
          
          if (data && data.state === 'InProgress' && data.playerResponse === 'None') {
            clearInterval(pollInterval);
            pollInterval = null;
            await attemptAcceptance();
          }
        }
      } catch (error) {
      }
    }, 300);
  };

  const stopPolling = () => {
    if (pollInterval) {
      clearInterval(pollInterval);
      pollInterval = null;
    }
  };

  const gameflowObserver = socket.observe('/lol-gameflow/v1/gameflow-phase', (message) => {
    const phase = message.data;
    
    if (phase === 'ReadyCheck') {
      setTimeout(startPolling, 500);
    } else {
      stopPolling();
      
      if (['None', 'Lobby', 'Matchmaking', 'ChampSelect'].includes(phase)) {
        if (hasAccepted || normalRetryCount > 0 || aggressiveRetryCount > 0) {
          resetState();
        }
      }
    }
  });

  const searchObserver = socket.observe('/lol-matchmaking/v1/search', (message) => {
    if (message.eventType === 'Delete' || 
        (message.data && message.data.searchState === 'Invalid')) {
      resetState();
      stopPolling();
    }
  });


  return () => {
    readyCheckObserver.disconnect();
    gameflowObserver.disconnect();
    searchObserver.disconnect();
    stopPolling();
  };
}