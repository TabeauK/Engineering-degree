<template>
  <div class="loader" />
</template>

<script>
import { mapGetters } from "vuex";
import { mapActions } from "vuex";

export default {
  name: "Verify",
  computed: mapGetters(["isLogged"]),
  mounted() {
    this.verifyToken({
      token: this.$route.query.oauth_token,
      pin: this.$route.query.oauth_verifier
    });
  },
  methods: {
    ...mapActions(["verifyToken"])
  }
};
</script>
<style scoped>
.loader {
  position: absolute;
  border: 30px solid #f3f3f3; /* Light grey */
  border-top: 30px solid #3498db; /* Blue */
  border-radius: 50%;
  left: calc(50% - 130px);
  top: calc(50% - 130px);
  width: 200px;
  height: 200px;
  animation: spin 2s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }

  100% {
    transform: rotate(360deg);
  }
}
</style>
