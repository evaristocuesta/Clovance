import { IconName } from "./icon-name";

export const ICONS: Record<IconName, string> = {
    'close': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18 17.94 6M18 18 6.06 6"/>
        </svg>
    `,
    'check': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 11.917 9.724 16.5 19 7.5"/>
        </svg>
    `,
    'trash-bin': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24">
            <path fill-rule="evenodd" d="M8.586 2.586A2 2 0 0 1 10 2h4a2 2 0 0 1 2 2v2h3a1 1 0 1 1 0 2v12a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V8a1 1 0 0 1 0-2h3V4a2 2 0 0 1 .586-1.414ZM10 6h4V4h-4v2Zm1 4a1 1 0 1 0-2 0v8a1 1 0 1 0 2 0v-8Zm4 0a1 1 0 1 0-2 0v8a1 1 0 1 0 2 0v-8Z" clip-rule="evenodd"/>
        </svg>
    `,
    'user-add': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24">
            <path fill-rule="evenodd" d="M9 4a4 4 0 1 0 0 8 4 4 0 0 0 0-8Zm-2 9a4 4 0 0 0-4 4v1a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2v-1a4 4 0 0 0-4-4H7Zm8-1a1 1 0 0 1 1-1h1v-1a1 1 0 1 1 2 0v1h1a1 1 0 1 1 0 2h-1v1a1 1 0 1 1-2 0v-1h-1a1 1 0 0 1-1-1Z" clip-rule="evenodd"/>
        </svg>
    `,
    'user-settings': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24">
            <path fill-rule="evenodd" d="M17 10v1.126c.367.095.714.24 1.032.428l.796-.797 1.415 1.415-.797.796c.188.318.333.665.428 1.032H21v2h-1.126c-.095.367-.24.714-.428 1.032l.797.796-1.415 1.415-.796-.797a3.979 3.979 0 0 1-1.032.428V20h-2v-1.126a3.977 3.977 0 0 1-1.032-.428l-.796.797-1.415-1.415.797-.796A3.975 3.975 0 0 1 12.126 16H11v-2h1.126c.095-.367.24-.714.428-1.032l-.797-.796 1.415-1.415.796.797A3.977 3.977 0 0 1 15 11.126V10h2Zm.406 3.578.016.016c.354.358.574.85.578 1.392v.028a2 2 0 0 1-3.409 1.406l-.01-.012a2 2 0 0 1 2.826-2.83ZM5 8a4 4 0 1 1 7.938.703 7.029 7.029 0 0 0-3.235 3.235A4 4 0 0 1 5 8Zm4.29 5H7a4 4 0 0 0-4 4v1a2 2 0 0 0 2 2h6.101A6.979 6.979 0 0 1 9 15c0-.695.101-1.366.29-2Z" clip-rule="evenodd"/>
        </svg>
    `,
    'users': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="currentColor" viewBox="0 0 24 24">
            <path fill-rule="evenodd" d="M8 4a4 4 0 1 0 0 8 4 4 0 0 0 0-8Zm-2 9a4 4 0 0 0-4 4v1a2 2 0 0 0 2 2h8a2 2 0 0 0 2-2v-1a4 4 0 0 0-4-4H6Zm7.25-2.095c.478-.86.75-1.85.75-2.905a5.973 5.973 0 0 0-.75-2.906 4 4 0 1 1 0 5.811ZM15.466 20c.34-.588.535-1.271.535-2v-1a5.978 5.978 0 0 0-1.528-4H18a4 4 0 0 1 4 4v1a2 2 0 0 1-2 2h-4.535Z" clip-rule="evenodd"/>
        </svg>
    `,
    'user-circle': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18Zm0 0a8.949 8.949 0 0 0 4.951-1.488A3.987 3.987 0 0 0 13 16h-2a3.987 3.987 0 0 0-3.951 3.512A8.948 8.948 0 0 0 12 21Zm3-11a3 3 0 1 1-6 0 3 3 0 0 1 6 0Z"/>
        </svg>
    `,
    'arrow-right-to-bracket': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M20 12H8m12 0-4 4m4-4-4-4M9 4H7a3 3 0 0 0-3 3v10a3 3 0 0 0 3 3h2"/>
        </svg>
    `,
    'arrow-down-to-bracket': `
        <svg aria-hidden="true" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 15v2a3 3 0 0 0 3 3h10a3 3 0 0 0 3-3v-2m-8 1V4m0 12-4-4m4 4 4-4"/>
        </svg>
    `,
    'arrow-up-from-bracket': `
        <svg aria-hidden="true" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 15v2a3 3 0 0 0 3 3h10a3 3 0 0 0 3-3v-2M12 4v12m0-12 4 4m-4-4L8 8"/>
        </svg>
    `,
    'sort-horizontal': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 16h13M4 16l4-4m-4 4 4 4M20 8H7m13 0-4 4m4-4-4-4"/>
        </svg>
    `,
    'wallet': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 8H5m12 0a1 1 0 0 1 1 1v2.6M17 8l-4-4M5 8a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-2.6M5 8l4-4 4 4m6 4h-4a2 2 0 1 0 0 4h4a1 1 0 0 0 1-1v-2a1 1 0 0 0-1-1Z"/>
        </svg>
    `,
    'chart-line-up': `
        <svg aria-hidden="true" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 4.5V19a1 1 0 0 0 1 1h15M7 14l4-4 4 4 5-5m0 0h-3.207M20 9v3.207"/>
        </svg>
    `,
    'moon': `
        <svg fill="currentColor" viewBox="0 0 20 20">
            <path d="M17.293 13.293A8 8 0 016.707 2.707a8.001 8.001 0 1010.586 10.586z"></path>
        </svg>
    `, 
    'sun': `
        <svg fill="currentColor" viewBox="0 0 20 20">
            <path d="M10 2a1 1 0 011 1v1a1 1 0 11-2 0V3a1 1 0 011-1zm4 8a4 4 0 11-8 0 4 4 0 018 0zm-.464 4.95l.707.707a1 1 0 001.414-1.414l-.707-.707a1 1 0 00-1.414 1.414zm2.12-10.607a1 1 0 010 1.414l-.706.707a1 1 0 11-1.414-1.414l.707-.707a1 1 0 011.414 0zM17 11a1 1 0 100-2h-1a1 1 0 100 2h1zm-7 4a1 1 0 011 1v1a1 1 0 11-2 0v-1a1 1 0 011-1zM5.05 6.464A1 1 0 106.465 5.05l-.708-.707a1 1 0 00-1.414 1.414l.707.707zm1.414 8.486l-.707.707a1 1 0 01-1.414-1.414l.707-.707a1 1 0 011.414 1.414zM4 11a1 1 0 100-2H3a1 1 0 000 2h1z" fill-rule="evenodd" clip-rule="evenodd"></path>
        </svg>
    `,
    'flag-es': `
        <svg version="1.1" viewBox="0 0 256 256" xml:space="preserve">
            <g style="stroke: none; stroke-width: 0; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: none; fill-rule: nonzero; opacity: 1;" transform="translate(1.4065934065934016 1.4065934065934016) scale(2.81 2.81)">
	            <path d="M 0 45 c 0 8.199 2.202 15.88 6.034 22.5 h 77.932 C 87.798 60.88 90 53.199 90 45 c 0 -8.199 -2.202 -15.88 -6.034 -22.5 H 6.034 C 2.202 29.12 0 36.801 0 45 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(255,196,0); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
	            <path d="M 83.966 22.5 c -0.269 -0.465 -0.546 -0.926 -0.831 -1.381 C 75.176 8.438 61.077 0 45 0 c -8.201 0 -15.881 2.205 -22.5 6.04 c -6.357 3.682 -11.736 8.867 -15.635 15.08 C 6.58 21.574 6.304 22.035 6.034 22.5 H 83.966 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(198,11,30); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
	            <path d="M 6.034 67.5 c 0.269 0.465 0.546 0.926 0.831 1.38 c 3.899 6.213 9.278 11.397 15.635 15.08 C 29.119 87.795 36.799 90 45 90 c 16.077 0 30.176 -8.438 38.135 -21.12 c 0.285 -0.455 0.562 -0.915 0.831 -1.38 H 6.034 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(198,11,30); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
            </g>
        </svg>
    `,
    'flag-en': `
        <svg version="1.1" viewBox="0 0 256 256" xml:space="preserve">
            <g style="stroke: none; stroke-width: 0; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: none; fill-rule: nonzero; opacity: 1;" transform="translate(1.4065934065934016 1.4065934065934016) scale(2.81 2.81)">
                <path d="M 88.35 57.052 c 0.034 -0.123 0.076 -0.243 0.109 -0.367 l -0.004 -0.002 C 89.457 52.957 90 49.043 90 45 c 0 -4.033 -0.54 -7.938 -1.538 -11.657 l 0.004 -0.002 c -0.039 -0.146 -0.088 -0.289 -0.128 -0.434 c -0.137 -0.492 -0.28 -0.982 -0.434 -1.468 c -0.081 -0.257 -0.167 -0.512 -0.253 -0.768 c -0.073 -0.217 -0.139 -0.437 -0.215 -0.653 h -0.015 c -1.645 -4.653 -4.021 -8.96 -7.01 -12.768 L 59.997 27.458 V 2.57 c -4.368 -1.544 -9.046 -2.427 -13.915 -2.542 h -2.164 c -4.868 0.115 -9.545 0.998 -13.913 2.541 v 24.889 L 9.589 17.249 c -2.989 3.809 -5.366 8.116 -7.01 12.769 H 2.564 c -0.076 0.216 -0.143 0.436 -0.216 0.653 c -0.086 0.255 -0.172 0.509 -0.253 0.765 c -0.154 0.486 -0.297 0.977 -0.434 1.47 c -0.04 0.145 -0.089 0.287 -0.128 0.432 l 0.004 0.002 C 0.54 37.061 0 40.966 0 45 c 0 4.043 0.543 7.957 1.545 11.684 l -0.004 0.002 c 0.033 0.123 0.074 0.242 0.108 0.365 c 0.146 0.524 0.298 1.046 0.462 1.562 c 0.075 0.236 0.154 0.47 0.233 0.705 c 0.077 0.231 0.148 0.464 0.229 0.693 H 2.59 c 1.647 4.651 4.025 8.955 7.016 12.761 l 20.4 -10.2 v 24.86 C 34.697 89.089 39.741 90 45 90 c 5.26 0 10.305 -0.911 14.997 -2.57 V 62.572 l 20.398 10.199 c 2.991 -3.806 5.368 -8.11 7.015 -12.76 h 0.015 c 0.081 -0.229 0.152 -0.463 0.23 -0.694 c 0.079 -0.234 0.158 -0.468 0.233 -0.704 C 88.052 58.096 88.205 57.575 88.35 57.052 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(243,244,245); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 53.999 0.902 c -2.565 -0.521 -5.213 -0.81 -7.917 -0.874 h -2.164 c -2.703 0.064 -5.35 0.354 -7.914 0.874 v 35.116 H 0.899 C 0.311 38.92 0 41.924 0 45 c 0 3.087 0.312 6.1 0.904 9.012 h 35.1 v 35.087 C 38.911 89.689 41.919 90 45 90 c 3.082 0 6.091 -0.311 8.999 -0.902 V 54.012 h 35.097 C 89.688 51.1 90 48.087 90 45 c 0 -3.076 -0.311 -6.08 -0.899 -8.983 H 53.999 V 0.902 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(204,0,0); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 85.242 65.135 c 0.829 -1.653 1.56 -3.363 2.184 -5.125 H 74.993 L 85.242 65.135 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(204,0,0); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 82.216 19.701 L 61.581 30.019 h 13.412 l 10.261 -5.131 C 84.353 23.088 83.341 21.354 82.216 19.701 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(204,0,0); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 4.747 24.887 c -0.829 1.655 -1.559 3.368 -2.182 5.132 H 15.01 L 4.747 24.887 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(204,0,0); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 7.8 70.321 L 28.422 60.01 H 15.01 L 4.758 65.136 C 5.661 66.936 6.674 68.67 7.8 70.321 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(204,0,0); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 9.605 72.771" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(0,0,102); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 80.412 17.251" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(0,0,102); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 80.395 72.77" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(0,0,102); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 9.589 17.25" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(0,0,102); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 9.589 17.249 l 20.416 10.208 v -3.99 V 2.584 C 21.874 5.458 14.813 10.593 9.589 17.249 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(0,0,102); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 59.997 2.585 v 22.302 v 2.57 L 80.411 17.25 C 75.188 10.594 68.128 5.459 59.997 2.585 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(0,0,102); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 30.006 72.77 V 62.572 l -20.4 10.2 c 5.222 6.646 12.276 11.774 20.4 14.646 V 72.77 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(0,0,102); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
                <path d="M 59.997 62.572 v 9.296 v 15.548 c 8.123 -2.872 15.176 -8 20.398 -14.646 L 59.997 62.572 z" style="stroke: none; stroke-width: 1; stroke-dasharray: none; stroke-linecap: butt; stroke-linejoin: miter; stroke-miterlimit: 10; fill: rgb(0,0,102); fill-rule: nonzero; opacity: 1;" transform=" matrix(1 0 0 1 0 0) " stroke-linecap="round"/>
            </g>
        </svg>
    `,
    'bars': `
        <svg aria-hidden="true"
            fill="currentColor"
            viewBox="0 0 20 20">
            <path fill-rule="evenodd"
                  d="M3 5a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1zM3 10a1 1 0 011-1h6a1 1 0 110 2H4a1 1 0 01-1-1zM3 15a1 1 0 011-1h12a1 1 0 110 2H4a1 1 0 01-1-1z"
                  clip-rule="evenodd"></path>
        </svg>
    `,
    'ellipsis-vertical': `
        <svg aria-hidden="true"
            fill="currentColor"
            viewBox="0 0 24 24">
            <path fill-rule="evenodd"
                  d="M12 5.5a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3Zm0 8a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3Zm0 8a1.5 1.5 0 1 1 0-3 1.5 1.5 0 0 1 0 3Z"
                  clip-rule="evenodd"></path>
        </svg>
    `,
    'edit': `
        <svg aria-hidden="true" 
            fill="none" 
            viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="m14.304 4.844 2.852 2.852M7 7H4a1 1 0 0 0-1 1v10a1 1 0 0 0 1 1h11a1 1 0 0 0 1-1v-4.5m2.409-9.91a2.017 2.017 0 0 1 0 2.853l-6.844 6.844L8 14l.713-3.565 6.844-6.844a2.015 2.015 0 0 1 2.852 0Z"/>
        </svg>
    `,
    'plus': `
        <svg aria-hidden="true" 
            fill="none" 
            viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 12h14m-7 7V5"/>
        </svg>
    `,
    'circle-plus': `
        <svg aria-hidden="true" 
            fill="none" 
            viewBox="0 0 24 24">
            <path stroke="currentColor" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 7.757v8.486M7.757 12h8.486M21 12a9 9 0 1 1-18 0 9 9 0 0 1 18 0Z"/>
        </svg>

    `,
    'filter': `
        <svg aria-hidden="true"
        fill="none" 
        viewBox="0 0 24 24">
          <path stroke="currentColor" stroke-linecap="round" stroke-width="2" d="M18.796 4H5.204a1 1 0 0 0-.753 1.659l5.302 6.058a1 1 0 0 1 .247.659v4.874a.5.5 0 0 0 .2.4l3 2.25a.5.5 0 0 0 .8-.4v-7.124a1 1 0 0 1 .247-.659l5.302-6.059c.566-.646.106-1.658-.753-1.658Z"/>
        </svg>

    `,
};
