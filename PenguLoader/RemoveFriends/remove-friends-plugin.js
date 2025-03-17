window.addEventListener('load', () => {
  const button = document.createElement('span');
  button.className = 'action-bar-button remove-all-friends-button';
  
  const style = document.createElement('style');
  style.textContent = `
    .remove-all-friends-button {
      position: relative;
      width: 32px;
      height: 32px;
      cursor: pointer;
      transition: background-image 0.2s;
      background-image: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32"><path fill="%23d0b071" d="M23.5 8.5 L8.5 23.5 M8.5 8.5 L23.5 23.5" stroke="%23d0b071" stroke-width="2"/></svg>');
      background-size: 20px;
      background-position: center;
      background-repeat: no-repeat;
    }
    
    .remove-all-friends-button:hover {
      background-image: url('data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32"><path fill="%23f0e6d2" d="M23.5 8.5 L8.5 23.5 M8.5 8.5 L23.5 23.5" stroke="%23f0e6d2" stroke-width="2"/></svg>');
    }
  `;
  document.head.appendChild(style);

  const removeAllFriends = async () => {
    try {
      if (!confirm('Are you sure you want to remove all friends?')) {
        return;
      }

      const response = await fetch('./lol-chat/v1/friends');
      if (!response.ok) {
        throw new Error(`Failed to fetch friends: ${response.status}`);
      }

      const friends = await response.json();

      const deletePromises = friends.map(friend => {
        const id = encodeURIComponent(friend.id);
        return fetch(`./lol-chat/v1/friends/${id}`, {
          method: 'DELETE'
        });
      });

      await Promise.all(deletePromises);
      console.log(`Successfully removed ${friends.length} friends`);
    } catch (error) {
      console.error('Error removing friends:', error);
    }
  };

  button.addEventListener('click', removeAllFriends);

  const addButton = () => {
    const buttonsContainer = document.querySelector('.actions-bar .buttons');
    if (buttonsContainer) {
      buttonsContainer.appendChild(button);
      return true;
    }
    return false;
  };

  if (!addButton()) {
    let attempts = 0;
    const maxAttempts = 5;
    const interval = setInterval(() => {
      if (addButton() || ++attempts >= maxAttempts) {
        clearInterval(interval);
      }
    }, 1000);
  }
});
