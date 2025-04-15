function toggleHeart(event, icon, toolId) {
    event.stopPropagation();
    event.preventDefault();
    
    var isUserFavourite = icon.classList.contains("liked");

    var url = isUserFavourite ? '/Favourite/RemoveFromFavourites' : '/Favourite/AddToFavourites';

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(toolId)
    })
    .then(response => {
        if (response.ok) {
            icon.classList.toggle("liked");

            var tooltipText = icon.classList.contains("liked") ? "Remove from favorites" : "Add to favorites";
            icon.setAttribute('title', tooltipText);
            icon.setAttribute('data-bs-original-title', tooltipText);
            var tooltip = bootstrap.Tooltip.getInstance(icon);
            if (tooltip) {
                tooltip.dispose(); 
            }
            new bootstrap.Tooltip(icon);

            const allIcons = document.querySelectorAll(`.heart-icon[data-tool-id="${toolId}"]`);
            allIcons.forEach(el => {
                if (el !== icon) {
                    el.classList.toggle("liked", icon.classList.contains("liked"));
                    el.setAttribute('title', tooltipText);
                    el.setAttribute('data-bs-original-title', tooltipText);
                    const t = bootstrap.Tooltip.getInstance(el);
                    if (t) t.dispose();
                    new bootstrap.Tooltip(el);
                }
            });

            updateFavouriteTools(toolId, icon.classList.contains("liked"));
            updateFavouriteToolsSidebar(toolId, icon.classList.contains("liked"));
        } else if (response.status === 401) {
            window.location.href = '/Auth/Login'; 
        } else {
            alert("Something went wrong!");
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert("Error processing the request.");
    });
}

function updateFavouriteTools(toolId, isAdding) {
    const allTools = document.querySelectorAll(`.tool-card-wrapper[data-tool-id="${toolId}"]`);
    const favouriteContainer = document.querySelector('.favourite-tools-container .row');

    if (!favouriteContainer) return;

    if (isAdding) {
        const alreadyExists = favouriteContainer.querySelector(`[data-tool-id="${toolId}"]`);
        if (!alreadyExists && allTools.length > 0) {
            const toolToAdd = allTools[0].cloneNode(true);
            toolToAdd.classList.add('fade-slide-in');
            favouriteContainer.appendChild(toolToAdd);
            checkFavouriteToolsEmpty();

            const newIcon = toolToAdd.querySelector('[data-bs-toggle="tooltip"]');
            if (newIcon) new bootstrap.Tooltip(newIcon);
        }
    } else {
        const toRemove = favouriteContainer.querySelector(`[data-tool-id="${toolId}"]`);
        if (toRemove) {
            toRemove.classList.add('fade-slide-out');
            setTimeout(() => {
                toRemove.remove();
                checkFavouriteToolsEmpty();
            }, 300); // Thời gian trùng với animation duration
        }
    }
}

function checkFavouriteToolsEmpty() {
    const favouriteList = document.getElementById('favourite-tools-list');
    const emptyMessage = document.getElementById('no-favourite-message');

    if (!favouriteList || !emptyMessage) return;

    if (favouriteList.children.length === 0) {
        emptyMessage.classList.remove('d-none');
    } else {
        emptyMessage.classList.add('d-none');
    }
}

function updateFavouriteToolsSidebar(toolId, isAdding) {
    var favoriteToolsMenuItem = document.getElementById('favorite-menu-item');
    var favoriteToolsListSideBar = document.querySelector('#FavoriteMenu ul');
    if (!favoriteToolsListSideBar) return;

    if (isAdding) {
        // Kiểm tra xem tool có trong sidebar chưa
        const alreadyExists = favoriteToolsListSideBar.querySelector(`[data-tool-id="${toolId}"]`);
        if (!alreadyExists) {
            // Lấy tool từ danh sách yêu thích
            const toolElement = document.querySelector(`.tool-card-wrapper[data-tool-id="${toolId}"]`);
            if (toolElement) {
                var toolName = toolElement.getAttribute('data-tool-name');
                var toolSlugName = toolElement.getAttribute('data-tool-slugname');
                
                // Tạo phần tử li mới và thêm vào sidebar
                var li = document.createElement('li');
                li.className = 'nav-item';
                li.setAttribute('data-tool-id', toolId);
                li.innerHTML = '<a class="nav-link fw-bold  " href="/' + toolSlugName + '">' + toolName + '</a>';

                // Thêm phần tử vào sidebar
                favoriteToolsListSideBar.appendChild(li);
                if (favoriteToolsMenuItem.classList.contains('d-none')) {
                    favoriteToolsMenuItem.classList.remove('d-none');
                }
            }
        }
    } else {
        // Nếu xóa tool yêu thích
        const toolToRemove = favoriteToolsListSideBar.querySelector(`[data-tool-id="${toolId}"]`);
        if (toolToRemove) {
            toolToRemove.remove();
            if (favoriteToolsListSideBar.children.length === 0) {
                favoriteToolsMenuItem.classList.add('d-none');
            }
        }
    }
}
