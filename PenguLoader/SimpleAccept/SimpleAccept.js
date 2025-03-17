export function init({ socket }) {
  let hasAccepted = false;

  socket.observe('/lol-matchmaking/v1/ready-check', async (message) => {

    if (message.eventType === 'Delete') {
      hasAccepted = false;
      return;
    }

    if (message.eventType === 'Update' && message.data) {
      const { playerResponse, state } = message.data;
      
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

  console.log('Auto accept initialized!');
}
