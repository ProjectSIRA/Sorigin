import preprocess from 'svelte-preprocess'
import node from '@sveltejs/adapter-node'

/** @type {import('@sveltejs/kit').Config} */
const config = {
	// Consult https://github.com/sveltejs/svelte-preprocess
	// for more information about preprocessors
	preprocess: preprocess({
		scss: {
			prependData: `@import 'src/styles/main.scss';`
		}
	}),

	kit: {
		// hydrate the <div id="svelte"> element in src/app.html
		target: '#svelte',
		adapter: node(),
		vite: {
			optimizeDeps: {
				exclude: ['sswr']
			}
		}
	}
};

export default config;
