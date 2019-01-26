until ./StatusBot; do
    echo "StatusBot crashed with exit code $?.  Respawning.." >&2
    sleep 1
done
