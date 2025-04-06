export function init({ socket }) {
  // Tracks if the current ready check has been accepted by this plugin
  let hasAccepted = false;

  // Resets state for the next queue cycle
  const resetState = () => {
    hasAccepted = false;
    // Optional: Add console.log here if you want feedback on reset
    // console.log('SimpleAccept: Ready for next queue');
  };

  // Observe ready check events
  socket.observe('/lol-matchmaking/v1/ready-check', async (message) => {
    if (message.eventType === 'Delete') {
      // Ready check was cancelled or completed fully
      resetState();
      return;
    }

    if (message.eventType === 'Update' && message.data) {
      const { playerResponse, state } = message.data;

      // Check if a ready check is active, we haven't accepted it yet,
      // and the client is waiting for our response
      if (state === 'InProgress' && !hasAccepted && playerResponse === 'None') {
        // Delay acceptance slightly to potentially avoid conflicts with
        // performance-intensive UI plugins loading during ready check.
        await new Promise((resolve) => setTimeout(resolve, 150));

        // Double-check acceptance status after delay, in case of manual interaction
        // or race conditions (though less likely with the flag).
        if (!hasAccepted) {
          try {
            const response = await fetch(
              '/lol-matchmaking/v1/ready-check/accept',
              {
                method: 'POST',
              }
            );

            if (response.ok) {
              // Successfully accepted
              hasAccepted = true;
              console.log('Match accepted automatically!');
            } else {
              // Log API errors (e.g., 4xx, 5xx)
              console.error(
                `SimpleAccept: Failed to accept match - Status ${response.status}`
              );
              // Optionally reset state on failure if desired
              // resetState();
            }
          } catch (error) {
            // Log network or other fetch errors
            console.error('SimpleAccept: Failed to accept match - Error:', error);
            // Optionally reset state on failure if desired
            // resetState();
          }
        }
      } else if (state !== 'InProgress' && hasAccepted) {
        // If the state changes from InProgress (e.g., EveryoneReady, Declined, Invalid)
        // after we have accepted, reset the flag for the next potential check.
        // This handles cases where Delete might be delayed.
        hasAccepted = false;
      }
    }
  });

  // Observe gameflow phase changes for robust state reset between queues/games
  socket.observe('/lol-gameflow/v1/gameflow-phase', (message) => {
    const phase = message.data;
    // Reset when out of queue/champ select (e.g., back in Lobby, Matchmaking, or None)
    if (phase === 'None' || phase === 'Lobby' || phase === 'Matchmaking') {
      // Only reset if the flag was true, preventing redundant resets
      if (hasAccepted) {
        resetState();
      }
    }
  });

  console.log('Auto accept initialized!');
}