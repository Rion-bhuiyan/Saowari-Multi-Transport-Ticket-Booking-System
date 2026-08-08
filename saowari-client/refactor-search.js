const fs = require('fs');

let content = fs.readFileSync('src/app/features/search/search.component.html', 'utf8');

// 1. Replace inline styles for tabs
content = content.replace(/\[style\.background\]="searchParams\.transportType === '([^']+)' \? '#00559F' : 'transparent'"/g, `[ngClass]="searchParams.transportType === '$1' ? 'bg-saowari-primary text-white' : 'bg-transparent text-gray-500 hover:text-saowari-text-primary'"`);
content = content.replace(/\[style\.color\]="searchParams\.transportType === '([^']+)' \? '#fff' : '#6b7280'"\n/g, ''); // Remove the color binding

// 2. Replace inline styles for trip type
content = content.replace(/\[style\.color\]="searchParams\.tripType === '([^']+)' \? '#00559F' : '#6b7280'"/g, `[ngClass]="searchParams.tripType === '$1' ? 'text-saowari-primary' : 'text-gray-500 hover:text-saowari-text-primary'"`);
content = content.replace(/\[style\.border-color\]="searchParams\.tripType === '([^']+)' \? '#00559F' : '#d1d5db'"/g, `[ngClass]="searchParams.tripType === '$1' ? 'border-saowari-primary' : 'border-saowari-border'"`);
content = content.replace(/\[style\.background\]="searchParams\.tripType === '([^']+)' \? '#00559F' : 'transparent'"/g, `[ngClass]="searchParams.tripType === '$1' ? 'bg-saowari-primary' : 'transparent'"`);

// 3. Dropdowns
content = content.replace(/\[style\.border-color\]="fromDropdownOpen \? '#00559F' : '#e5e7eb'"/g, `[ngClass]="fromDropdownOpen ? 'border-saowari-primary' : 'border-saowari-border'"`);
content = content.replace(/\[style\.border-color\]="toDropdownOpen \? '#00559F' : '#e5e7eb'"/g, `[ngClass]="toDropdownOpen ? 'border-saowari-primary' : 'border-saowari-border'"`);
content = content.replace(/style="color: #00559F;"/g, `class="text-saowari-primary"`);

// 4. Dropdown Items
content = content.replace(/hover:bg-blue-50/g, 'hover:bg-saowari-surface-alt');
content = content.replace(/\[style\.background\]="searchParams\.fromLocationId === loc\.locationId\.toString\(\) \? '#e8f1fa' : ''"/g, `[ngClass]="searchParams.fromLocationId === loc.locationId.toString() ? 'bg-saowari-surface-alt' : ''"`);
content = content.replace(/\[style\.background\]="searchParams\.toLocationId === loc\.locationId\.toString\(\) \? '#e8f1fa' : ''"/g, `[ngClass]="searchParams.toLocationId === loc.locationId.toString() ? 'bg-saowari-surface-alt' : ''"`);

content = content.replace(/style="background: #e8f1fa; color: #00559F;"/g, `class="bg-saowari-primary-light text-saowari-primary"`);

// 5. Swap button
content = content.replace(/style="border-color: #00559F; color: #00559F;"\s+onmouseover="this\.style\.background='#00559F'; this\.style\.color='white';"\s+onmouseout="this\.style\.background=''; this\.style\.color='#00559F';"/g, `class="border-saowari-primary text-saowari-primary hover:bg-saowari-primary hover:text-white"`);

// 6. Search button
content = content.replace(/style="background: linear-gradient\(135deg, #00559F 0%, #0074d4 100%\); box-shadow: 0 8px 30px rgba\(0,85,159,0\.35\);"\s+onmouseover="this\.style\.background='linear-gradient\\(135deg, #00447f 0%, #005db0 100%\\)'"\s+onmouseout="this\.style\.background='linear-gradient\\(135deg, #00559F 0%, #0074d4 100%\\)'"/g, `class="bg-gradient-hero"`);

// 7. Date fields
content = content.replace(/\[style\.border-color\]="\(searchParams\.departureDate && searchParams\.tripType === 'one-way'\) \|\| \(searchParams\.departureDate && searchParams\.returnDate && searchParams\.tripType === 'round-way'\) \? '#00559F' : '#e5e7eb'"/g, `[ngClass]="(searchParams.departureDate && searchParams.tripType === 'one-way') || (searchParams.departureDate && searchParams.returnDate && searchParams.tripType === 'round-way') ? 'border-saowari-primary' : 'border-saowari-border'"`);

// 8. Various hardcoded colors in template
content = content.replace(/text-blue-500/g, 'text-saowari-primary');
content = content.replace(/text-blue-600/g, 'text-saowari-primary');
content = content.replace(/text-blue-700/g, 'text-saowari-primary');
content = content.replace(/text-blue-800/g, 'text-saowari-primary');
content = content.replace(/bg-blue-50/g, 'bg-saowari-primary-light');
content = content.replace(/border-blue-100/g, 'border-saowari-primary/30');
content = content.replace(/bg-\[\#00559F\]/g, 'bg-saowari-primary');
content = content.replace(/hover:bg-\[\#00447f\]/g, 'hover:bg-saowari-primary-dark');
content = content.replace(/bg-blue-500/g, 'bg-saowari-primary');
content = content.replace(/bg-green-100/g, 'bg-green-500/20');
content = content.replace(/bg-blue-100/g, 'bg-blue-500/20');
content = content.replace(/bg-orange-100/g, 'bg-orange-500/20');
content = content.replace(/text-green-800/g, 'text-green-500');
content = content.replace(/text-blue-800/g, 'text-blue-500');
content = content.replace(/text-orange-800/g, 'text-orange-500');

// 9. Remove shadow-[0_20px_60px_-10px_rgba(0,85,159,0.2)]
content = content.replace(/shadow-\[0_20px_60px_-10px_rgba\(0,85,159,0\.2\)\]/g, 'shadow-lg');
content = content.replace(/shadow-\[0_10px_40px_rgba\(0,85,159,0\.15\)\]/g, 'shadow-xl');
content = content.replace(/shadow-\[0_8px_32px_rgba\(0,85,159,0\.06\)\]/g, 'shadow-md');
content = content.replace(/hover:shadow-\[0_16px_48px_rgba\(0,85,159,0\.15\)\]/g, 'hover:shadow-xl');

// Write back
fs.writeFileSync('src/app/features/search/search.component.html', content);
console.log('Replacements done!');
