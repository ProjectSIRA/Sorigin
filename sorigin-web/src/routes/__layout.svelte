<nav class="container-fluid">
    <ul>
        <li><h3>Sorigin</h3></li>
    </ul>
    <ul>
        <li>Log In</li>
    </ul>
</nav>

<slot></slot>

<style lang="css" global>
	@media only screen and (prefers-color-scheme: dark) {
		:root:not([data-theme="light"]) {
            --background-color: #0e0e0e;
            --card-background-color: #1b1b1b;
		}
	}
</style>