export function init({ socket }) {
  let hasAccepted = false;
  let lastQueueId = null;

  const resetState = () => {
    hasAccepted = false;
    console.log('SimpleAccept: Ready for next queue');
  };

  socket.observe('/lol-matchmaking/v1/ready-check', async (message) => {
    if (message.eventType === 'Delete') {
      resetState();
      return;
    }

    if (message.eventType === 'Update' && message.data) {
      const { playerResponse, state, gameId } = message.data;
      
      if (gameId && gameId !== lastQueueId) {
        lastQueueId = gameId;
        hasAccepted = false;
      }
      
      if (!hasAccepted && state === 'InProgress' && playerResponse === 'None') {
        try {
          const response = await fetch('/lol-matchmaking/v1/ready-check/accept', {
            method: 'POST'
          });

          if (response.ok) {
            hasAccepted = true;
            console.log('Match accepted automatically!');
          }
        } catch (error) {
          console.error('Failed to accept match:', error);
        }
      }
    }
  });

  socket.observe('/lol-gameflow/v1/gameflow-phase', (message) => {
    const phase = message.data;
    if (phase === 'None' || phase === 'Lobby') {
      resetState();
    }
  });

  console.log('Auto accept initialized!');
}
