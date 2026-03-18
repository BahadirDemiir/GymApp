class SearchFilterManager {
    constructor(options = {}) {
        this.searchInput = document.getElementById('searchInput');
        this.filterChips = document.querySelectorAll('.filter-chip');
        this.sortSelect = document.getElementById('sortSelect');
        this.itemSelector = options.itemSelector || '.item';
        this.items = document.querySelectorAll(this.itemSelector);
        this.gridContainer = document.getElementById(options.gridId || 'grid');
        this.skeletonLoader = document.getElementById(options.skeletonId || 'skeletonLoader');
        this.emptyState = document.getElementById(options.emptyStateId || 'emptyState');
        
        this.currentFilter = 'all';
        this.currentSearch = '';
        this.sortOptions = options.sortOptions || {};
        
        this.itemsArray = Array.from(this.items);
        this.debounceTimer = null;
        this.isFiltering = false;

        this.itemData = this.itemsArray.map(item => ({
            element: item,
            category: item.dataset.category || item.dataset.specialty || '',
            level: item.dataset.level || '',
            name: (item.dataset.name || '').toLowerCase(),
            rating: parseFloat(item.querySelector('.fw-semibold')?.textContent || 0),
            experience: parseInt(item.querySelector('.stats-item:last-child span')?.textContent || 0),
            students: parseInt(item.querySelector('.stats-item:first-child span')?.textContent || 0),
            duration: parseFloat(item.querySelector('.meta-item:last-child span')?.textContent || 0),
            startDate: new Date(item.querySelector('.meta-item:first-child span')?.textContent || ''),
            popularity: this.extractPopularity(item)
        }));
        
        this.init();
    }
    
    extractPopularity(item) {
        const text = item.querySelector('.meta-item:nth-child(2) span')?.textContent || '0/0';
        return parseInt(text.split('/')[0] || 0);
    }
    
    init() {
        this.bindEvents();
        this.showLoading();
        this.preventDefaultNavigation();
    }
    
    preventDefaultNavigation() {
        this.filterChips.forEach(chip => {
            chip.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
            });
        });
    }
    
    bindEvents() {
        if (this.searchInput) {
            this.searchInput.addEventListener('input', (e) => {
                this.currentSearch = e.target.value.toLowerCase();
                this.debounceFilter();
            });
        }
        
        this.filterChips.forEach(chip => {
            chip.addEventListener('click', (e) => {
                if (this.isFiltering) return; 
                
                this.filterChips.forEach(c => c.classList.remove('active'));
                e.target.classList.add('active');
                this.currentFilter = e.target.dataset.filter || e.target.getAttribute('href')?.split('filter=')[1]?.split('&')[0] || 'all';
                this.filterItems();
            });
        });
        
        if (this.sortSelect) {
            this.sortSelect.addEventListener('change', (e) => {
                this.sortItems(e.target.value);
            });
        }
    }
    
    debounceFilter() {
        clearTimeout(this.debounceTimer);
        this.debounceTimer = setTimeout(() => {
            this.filterItems();
        }, 150); 
    }
    
    filterItems() {
        if (this.isFiltering) return;
        this.isFiltering = true;
        
        requestAnimationFrame(() => {
            let visibleCount = 0;
            const fragment = document.createDocumentFragment();
            
            this.itemData.forEach(itemData => {
                const matchesFilter = this.currentFilter === 'all' || 
                                    itemData.category.toLowerCase() === this.currentFilter.toLowerCase();
                const matchesSearch = this.currentSearch === '' || 
                                    itemData.name.includes(this.currentSearch);
                
                if (matchesFilter && matchesSearch) {
                    itemData.element.style.display = 'block';
                    fragment.appendChild(itemData.element);
                    visibleCount++;
                } else {
                    itemData.element.style.display = 'none';
                }
            });
            
            if (this.gridContainer) {
                this.gridContainer.appendChild(fragment);
            }
            
            this.toggleEmptyState(visibleCount === 0);
            this.isFiltering = false;
        });
    }
    
    sortItems(sortBy) {
        if (this.isFiltering) return;
        this.isFiltering = true;
        
        requestAnimationFrame(() => {
            const visibleItems = this.itemData.filter(itemData => 
                itemData.element.style.display !== 'none'
            );
            
            visibleItems.sort((a, b) => {
                let aValue, bValue;
                
                switch(sortBy) {
                    case 'rating':
                        aValue = a.rating;
                        bValue = b.rating;
                        return bValue - aValue;
                    case 'experience':
                        aValue = a.experience;
                        bValue = b.experience;
                        return bValue - aValue;
                    case 'students':
                        aValue = a.students;
                        bValue = b.students;
                        return bValue - aValue;
                    case 'duration':
                        aValue = a.duration;
                        bValue = b.duration;
                        return aValue - bValue;
                    case 'startDate':
                        aValue = a.startDate;
                        bValue = b.startDate;
                        return aValue - bValue;
                    case 'popular':
                        aValue = a.popularity;
                        bValue = b.popularity;
                        return bValue - aValue;
                    case 'name':
                        aValue = a.name;
                        bValue = b.name;
                        return aValue.localeCompare(bValue);
                    default:
                        return 0;
                }
            });
            
            const fragment = document.createDocumentFragment();
            visibleItems.forEach(itemData => {
                fragment.appendChild(itemData.element);
            });
            
            if (this.gridContainer) {
                this.gridContainer.appendChild(fragment);
            }
            
            this.isFiltering = false;
        });
    }
    
    toggleEmptyState(showEmpty) {
        if (showEmpty) {
            if (this.gridContainer) this.gridContainer.style.display = 'none';
            if (this.emptyState) this.emptyState.style.display = 'block';
        } else {
            if (this.gridContainer) this.gridContainer.style.display = 'flex';
            if (this.emptyState) this.emptyState.style.display = 'none';
        }
    }
    
    showLoading() {
        if (this.skeletonLoader) {
            this.skeletonLoader.style.display = 'flex';
            if (this.gridContainer) this.gridContainer.style.display = 'none';
            
            setTimeout(() => {
                this.skeletonLoader.style.display = 'none';
                if (this.gridContainer) this.gridContainer.style.display = 'flex';
            }, 400); 
        }
    }
    
    clearFilters() {
        if (this.searchInput) {
            this.searchInput.value = '';
        }
        
        this.filterChips.forEach(chip => chip.classList.remove('active'));
        const allChip = document.querySelector('.filter-chip[data-filter="all"]') || 
                       document.querySelector('.filter-chip[href*="filter=all"]');
        if (allChip) {
            allChip.classList.add('active');
        }
        
        if (this.sortSelect) {
            this.sortSelect.value = this.sortSelect.options[0].value;
        }
        
        this.currentFilter = 'all';
        this.currentSearch = '';
        
        this.itemData.forEach(itemData => {
            itemData.element.style.display = 'block';
        });
        
        if (this.gridContainer) this.gridContainer.style.display = 'flex';
        if (this.emptyState) this.emptyState.style.display = 'none';
    }
}

function clearFilters() {
    if (window.searchFilterManager) {
        window.searchFilterManager.clearFilters();
    }
}
